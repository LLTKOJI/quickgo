<#
 QuickGo - Prueba automatica de TODAS las historias que existen hoy.
 Uso (con la API corriendo y Postgres arriba):
     powershell -ExecutionPolicy Bypass -File .\probar-todo.ps1
 Parametros: -Base (URL de la API), -Contenedor (contenedor de Postgres), -Usuario, -Db, -SinEsperaCierre.
 Con el compose de infra/:  -Contenedor postgres-quickgo -Usuario quickgo
 Nota: el script usa "wsl docker exec"; si Docker corre nativo en Windows, quita "wsl" en la funcion Psql.
 Cada ejecucion crea empresas nuevas con datos unicos, asi que se puede repetir sin reiniciar la base.
#>
param(
    [string]$Base = "http://localhost:5010",
    [string]$Contenedor = "quickgo-pg",
    [string]$Usuario = "postgres",
    [string]$Db = "quickgo",
    [switch]$SinEsperaCierre
)

Add-Type -AssemblyName System.Net.Http
$ErrorActionPreference = "Continue"
$http = [System.Net.Http.HttpClient]::new()
$script:pass = 0; $script:fail = 0; $script:fallos = @()
$Clave = "Secreta123"
$s = "{0:D6}" -f (Get-Random -Maximum 999999)

function Seccion($t) { Write-Host ""; Write-Host "=== $t ===" -ForegroundColor Cyan }

function Registrar($ok, $nombre, $detalle) {
    if ($ok) { $script:pass++; Write-Host ("  PASS  " + $nombre) -ForegroundColor Green }
    else { $d = [string]$detalle; if ($d.Length -gt 230) { $d = $d.Substring(0, 230) + "..." }; $script:fail++; $script:fallos += ($nombre + "  ->  " + $d); Write-Host ("  FAIL  " + $nombre + "  ->  " + $d) -ForegroundColor Red }
}

# Llama a la API. $Esperado = lista de codigos HTTP aceptados.
function T {
    param([string]$Nombre, [string]$Metodo, [string]$Ruta, $Cuerpo = $null, [string]$Token = $null, [int[]]$Esperado = @(200))
    $req = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Metodo), ($Base + $Ruta))
    if ($Token) { $req.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token) }
    if ($null -ne $Cuerpo) {
        $json = $Cuerpo | ConvertTo-Json -Depth 8
        $req.Content = [System.Net.Http.StringContent]::new($json, [System.Text.Encoding]::UTF8, "application/json")
    }
    $status = 0; $texto = ""
    try {
        $resp = $http.SendAsync($req).GetAwaiter().GetResult()
        $status = [int]$resp.StatusCode
        $texto = $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    } catch { $texto = $_.Exception.Message }
    $obj = $null
    if ($texto) { try { $obj = $texto | ConvertFrom-Json } catch { } }
    $ok = $Esperado -contains $status
    $det = "esperaba " + ($Esperado -join "/") + " y devolvio " + $status
    if (-not $ok -and $texto) { $det += " :: " + $texto.Substring(0, [Math]::Min(180, $texto.Length)) }
    Registrar $ok $Nombre $det
    return [pscustomobject]@{ Status = $status; Body = $obj; Texto = $texto }
}

function Verificar($Nombre, $Condicion, $Detalle = "la condicion no se cumplio") { Registrar ([bool]$Condicion) $Nombre $Detalle }
function Cuenta($o) { if ($null -eq $o) { 0 } else { @($o).Count } }
function Dia($n) { (Get-Date).AddDays($n).ToString("yyyy-MM-dd") }
function Iso($d) { (Get-Date).ToUniversalTime().AddDays($d).ToString("yyyy-MM-ddTHH:mm:ssZ") }
function Psql($sql) { $sql | wsl docker exec -i $Contenedor psql -U $Usuario -d $Db -v ON_ERROR_STOP=1 2>&1 }

$rolAdmin = "a0000000-0000-0000-0000-000000000001"
$rolDesp  = "a0000000-0000-0000-0000-000000000002"
$rolContr = "a0000000-0000-0000-0000-000000000004"
$tipoCamion = "a23fa570-2f4b-4f85-80e9-042d0d315001"
$ciudadTegus = "33333333-0000-0000-0000-000000000001"
$ciudadSPS   = "33333333-0000-0000-0000-000000000002"

$emailA = "admin1.$s@transportes.hn"; $emailB = "admin.$s@otra.hn"
$emailC = "admin.$s@contratante.hn";  $emailD = "admin.$s@contratante2.hn"
$emailDesp = "desp.$s@transportes.hn"; $emailCond = "cond.$s@transportes.hn"

# ---------------------------------------------------------------
Seccion "PASO 0 - Datos base (monedas, Honduras, ciudades)"
$seed = @"
INSERT INTO monedas (codigo, nombre, simbolo) VALUES ('HNL','Lempira','L'),('USD','Dolar estadounidense','$') ON CONFLICT DO NOTHING;
INSERT INTO paises (id, nombre, codigo_iso) VALUES ('11111111-0000-0000-0000-000000000001','Honduras','HND') ON CONFLICT DO NOTHING;
INSERT INTO departamentos (id, pais_id, nombre) VALUES
 ('22222222-0000-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','Francisco Morazan'),
 ('22222222-0000-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','Cortes') ON CONFLICT DO NOTHING;
INSERT INTO ciudades (id, departamento_id, nombre, codigo_postal) VALUES
 ('33333333-0000-0000-0000-000000000001','22222222-0000-0000-0000-000000000001','Tegucigalpa','11101'),
 ('33333333-0000-0000-0000-000000000002','22222222-0000-0000-0000-000000000002','San Pedro Sula','21101') ON CONFLICT DO NOTHING;
"@
$r = Psql $seed
Verificar "seed.sql aplicado (Postgres en WSL responde)" ($LASTEXITCODE -eq 0) ($r | Out-String)

# ---------------------------------------------------------------
Seccion "0. Salud"
$null = T "0.1 GET /health" GET "/health" $null $null @(200)

# ---------------------------------------------------------------
Seccion "1. B1-01 Registro de empresa e inicio de sesion"
$regA = T "1.1 Registrar Transportista A" POST "/api/empresas/registro" @{ nombreComercial="Transportes Honduras $s"; razonSocial="Transportes Honduras SA"; rtn="0801999A$s"; tipo="Transportista"; primerNombre="Ana"; primerApellido="Lopez"; email=$emailA; password=$Clave } $null @(200,201)
$null = T "1.2 RTN repetido -> 409" POST "/api/empresas/registro" @{ nombreComercial="Otra $s"; razonSocial="Otra SA"; rtn="0801999A$s"; tipo="Transportista"; primerNombre="X"; primerApellido="Y"; email="rep.$s@x.hn"; password=$Clave } $null @(409)
$null = T "1.3 Contrasena debil -> 400" POST "/api/empresas/registro" @{ nombreComercial="Debil $s"; razonSocial="Debil SA"; rtn="0801999D$s"; tipo="Transportista"; primerNombre="X"; primerApellido="Y"; email="debil.$s@x.hn"; password="soloLetras" } $null @(400)
$null = T "1.4 Tipo de empresa invalido -> 400" POST "/api/empresas/registro" @{ nombreComercial="Mala $s"; razonSocial="Mala SA"; rtn="0801999M$s"; tipo="Otro"; primerNombre="X"; primerApellido="Y"; email="mala.$s@x.hn"; password=$Clave } $null @(400)
$null = T "1.5 Registrar Contratante C" POST "/api/empresas/registro" @{ nombreComercial="Constructora Cima $s"; razonSocial="Cima SA"; rtn="0801999C$s"; tipo="Contratante"; primerNombre="Carlos"; primerApellido="Mejia"; email=$emailC; password=$Clave } $null @(200,201)
$null = T "1.6 Registrar Transportista B" POST "/api/empresas/registro" @{ nombreComercial="Fletes del Norte $s"; razonSocial="Fletes SA"; rtn="0801999B$s"; tipo="Transportista"; primerNombre="Beto"; primerApellido="Rivas"; email=$emailB; password=$Clave } $null @(200,201)
$null = T "1.7 Registrar Contratante D" POST "/api/empresas/registro" @{ nombreComercial="Inmobiliaria Sol $s"; razonSocial="Sol SA"; rtn="0801999E$s"; tipo="Contratante"; primerNombre="Dora"; primerApellido="Pineda"; email=$emailD; password=$Clave } $null @(200,201)

$lA = T "1.8 Login A (token, rol Admin)" POST "/api/auth/login" @{ email=$emailA; password=$Clave } $null @(200)
Verificar "1.8b login devuelve token y rol=Admin" ($lA.Body.token -and $lA.Body.rol -eq "Admin") ($lA.Texto)
$tA = $lA.Body.token
$lB = T "1.9 Login B" POST "/api/auth/login" @{ email=$emailB; password=$Clave } $null @(200); $tB = $lB.Body.token
$lC = T "1.10 Login C (Contratante)" POST "/api/auth/login" @{ email=$emailC; password=$Clave } $null @(200); $tC = $lC.Body.token
$lD = T "1.11 Login D (Contratante 2)" POST "/api/auth/login" @{ email=$emailD; password=$Clave } $null @(200); $tD = $lD.Body.token
$null = T "1.11b Admin de C crea un usuario con rol Contratante" POST "/api/usuarios" @{ primerNombre="Cora"; primerApellido="Lagos"; email="contr.$s@contratante.hn"; password=$Clave; rolId=$rolContr } $tC @(200,201)
$null = T "1.11c Admin de D crea un usuario con rol Contratante" POST "/api/usuarios" @{ primerNombre="Dario"; primerApellido="Vega"; email="contr.$s@contratante2.hn"; password=$Clave; rolId=$rolContr } $tD @(200,201)
$lC2 = T "1.11d Login usuario Contratante de C" POST "/api/auth/login" @{ email="contr.$s@contratante.hn"; password=$Clave } $null @(200); $tC2 = $lC2.Body.token
Verificar "1.11e el rol es Contratante" ($lC2.Body.rol -eq "Contratante") $lC2.Texto
$lD2 = T "1.11f Login usuario Contratante de D" POST "/api/auth/login" @{ email="contr.$s@contratante2.hn"; password=$Clave } $null @(200); $tD2 = $lD2.Body.token
$w1 = T "1.12 Contrasena incorrecta -> 401" POST "/api/auth/login" @{ email=$emailA; password="Incorrecta1" } $null @(401)
$w2 = T "1.13 Correo inexistente -> 401" POST "/api/auth/login" @{ email="noexiste.$s@x.hn"; password=$Clave } $null @(401)
Verificar "1.13b mismo mensaje para correo y contrasena" ($w1.Body.detail -eq $w2.Body.detail) ("detalles distintos: " + $w1.Body.detail + " / " + $w2.Body.detail)

# ---------------------------------------------------------------
Seccion "2. B1-02 Contexto de empresa y autorizacion por rol"
$null = T "2.1 Sin token -> 401" GET "/api/usuarios" $null $null @(401)
$uA = T "2.2 Admin A lista usuarios" GET "/api/usuarios" $null $tA @(200)
Verificar "2.2b A ve solo 1 usuario" ((Cuenta $uA.Body) -eq 1) $uA.Texto
$anaId = @($uA.Body)[0].id
$uB = T "2.3 Admin B lista usuarios" GET "/api/usuarios" $null $tB @(200)
Verificar "2.3b B ve solo 1 usuario (no ve a Ana)" ((Cuenta $uB.Body) -eq 1 -and @($uB.Body)[0].email -eq $emailB) $uB.Texto
$null = T "2.4 Usuario Contratante intenta listar usuarios -> 403" GET "/api/usuarios" $null $tC2 @(403)

# ---------------------------------------------------------------
Seccion "3. B1-03 Roles y usuarios"
$roles = T "3.1 Catalogo de roles" GET "/api/catalogos/roles" $null $tA @(200)
Verificar "3.1b hay 4 roles" ((Cuenta $roles.Body) -eq 4) $roles.Texto
$cd = T "3.2 Crear Despachador" POST "/api/usuarios" @{ primerNombre="Diego"; primerApellido="Cruz"; telefono="9999-0001"; email=$emailDesp; password=$Clave; rolId=$rolDesp } $tA @(200,201)
$despId = $cd.Body.id
$null = T "3.3 Email repetido -> 409" POST "/api/usuarios" @{ primerNombre="Otro"; primerApellido="Cruz"; email=$emailDesp; password=$Clave; rolId=$rolDesp } $tA @(409)
$null = T "3.4 Rol Contratante en Transportista -> 400" POST "/api/usuarios" @{ primerNombre="Mal"; primerApellido="Rol"; email="malrol.$s@transportes.hn"; password=$Clave; rolId=$rolContr } $tA @(400,409)
$null = T "3.5 Rol Despachador en Contratante -> 400" POST "/api/usuarios" @{ primerNombre="Mal"; primerApellido="Rol"; email="malrol.$s@contratante.hn"; password=$Clave; rolId=$rolDesp } $tC @(400,409)
$lDesp = T "3.6 Login Despachador" POST "/api/auth/login" @{ email=$emailDesp; password=$Clave } $null @(200); $tDesp = $lDesp.Body.token
$null = T "3.7 Despachador crea usuarios -> 403" POST "/api/usuarios" @{ primerNombre="No"; primerApellido="Puede"; email="nopuede.$s@x.hn"; password=$Clave; rolId=$rolDesp } $tDesp @(403)
$null = T "3.8 Editar Despachador" PUT "/api/usuarios/$despId" @{ primerNombre="Diego"; segundoNombre="Andres"; primerApellido="Cruz"; telefono="9999-0002"; rolId=$rolDesp } $tA @(200)
$null = T "3.9 B edita usuario de A -> 404" PUT "/api/usuarios/$despId" @{ primerNombre="X"; primerApellido="Y"; rolId=$rolDesp } $tB @(404)
$ct = T "3.10 Crear usuario temporal" POST "/api/usuarios" @{ primerNombre="Temp"; primerApellido="Usuario"; email="temp.$s@transportes.hn"; password=$Clave; rolId=$rolDesp } $tA @(200,201)
$tempId = $ct.Body.id
$null = T "3.11 Desactivar temporal" PATCH "/api/usuarios/$tempId/desactivar" $null $tA @(200,204)
$null = T "3.12 Desactivado no puede entrar -> 403" POST "/api/auth/login" @{ email="temp.$s@transportes.hn"; password=$Clave } $null @(403)
$null = T "3.13 Admin se desactiva a si mismo -> 400/409" PATCH "/api/usuarios/$anaId/desactivar" $null $tA @(400,409)

# ---------------------------------------------------------------
Seccion "4. B1-04 y B1-05 Conductores"
$c1 = T "4.1 Conductor interno" POST "/api/conductores" @{ primerNombre="Pedro"; primerApellido="Ramos"; telefono="9999-1111"; email=$emailCond; password=$Clave; tipoConductor="Interno"; licencia="LIC-0001"; venceLicencia=(Dia 365) } $tDesp @(200,201)
$cond1Id = $c1.Body.id
$c2 = T "4.2 Conductor externo (sin acceso)" POST "/api/conductores" @{ primerNombre="Luis"; primerApellido="Mendoza"; tipoConductor="Externo"; licencia="LIC-0002"; venceLicencia=(Dia 15) } $tDesp @(200,201)
$cond2Id = $c2.Body.id
Verificar "4.2b externo con tieneAcceso=false" ($c2.Body.tieneAcceso -eq $false) $c2.Texto
$null = T "4.3 Conductor con licencia vencida" POST "/api/conductores" @{ primerNombre="Sara"; primerApellido="Flores"; tipoConductor="Externo"; licencia="LIC-0003"; venceLicencia=(Dia -2) } $tDesp @(200,201)
$null = T "4.4 Licencia repetida -> 409" POST "/api/conductores" @{ primerNombre="Rep"; primerApellido="Etido"; tipoConductor="Externo"; licencia="LIC-0001"; venceLicencia=(Dia 100) } $tDesp @(409)
$null = T "4.5 Interno sin email/password -> 400" POST "/api/conductores" @{ primerNombre="Sin"; primerApellido="Acceso"; tipoConductor="Interno"; licencia="LIC-0004"; venceLicencia=(Dia 100) } $tDesp @(400)
$lc = T "4.6 Listar conductores" GET "/api/conductores" $null $tDesp @(200)
Verificar "4.6b hay 3 conductores" ((Cuenta $lc.Body) -eq 3) $lc.Texto
$lx = T "4.7 Filtrar por tipo Externo" GET "/api/conductores?tipoConductor=Externo" $null $tDesp @(200)
Verificar "4.7b hay 2 externos" ((Cuenta $lx.Body) -eq 2) $lx.Texto
$lv = T "4.8 Licencias por vencer (30 dias)" GET "/api/conductores/licencias-por-vencer?dias=30" $null $tDesp @(200)
Verificar "4.8b incluye 2 (una ya vencida marcada)" ((Cuenta $lv.Body) -eq 2 -and ([regex]::Matches($lv.Texto, '"vencida":true')).Count -eq 1) $lv.Texto
$null = T "4.9 Detalle de conductor" GET "/api/conductores/$cond1Id" $null $tDesp @(200)
$null = T "4.10 Editar conductor" PUT "/api/conductores/$cond1Id" @{ primerNombre="Pedro"; segundoNombre="Jose"; primerApellido="Ramos"; telefono="9999-2222"; licencia="LIC-0001"; venceLicencia=(Dia 400); estado="Activo" } $tDesp @(200)
$null = T "4.11 B consulta conductor de A -> 404" GET "/api/conductores/$cond1Id" $null $tB @(404)
$lCond = T "4.12 Login conductor interno" POST "/api/auth/login" @{ email=$emailCond; password=$Clave } $null @(200); $tCond = $lCond.Body.token
$own = T "4.13 Conductor lista: solo su perfil" GET "/api/conductores" $null $tCond @(200)
Verificar "4.13b ve solo 1 (el suyo)" ((Cuenta $own.Body) -eq 1) $own.Texto
$null = T "4.14 Conductor ve licencias por vencer -> 403" GET "/api/conductores/licencias-por-vencer" $null $tCond @(403)

# ---------------------------------------------------------------
Seccion "5. B3-01 Catalogos"
$pa = T "5.1 Paises" GET "/api/catalogos/paises" $null $tA @(200)
Verificar "5.1b incluye Honduras" ($pa.Texto -match "Honduras") $pa.Texto
$d = T "5.2 Departamentos de Honduras" GET "/api/catalogos/departamentos?paisId=11111111-0000-0000-0000-000000000001" $null $tA @(200)
Verificar "5.2b hay departamentos" ((Cuenta $d.Body) -ge 2) $d.Texto
$ci = T "5.3 Ciudades de Francisco Morazan" GET "/api/catalogos/ciudades?departamentoId=22222222-0000-0000-0000-000000000001" $null $tA @(200)
Verificar "5.3b hay ciudades" ((Cuenta $ci.Body) -ge 1) $ci.Texto
$m = T "5.4 Monedas" GET "/api/catalogos/monedas" $null $tA @(200)
Verificar "5.4b incluye HNL y USD" ((Cuenta $m.Body) -ge 2) $m.Texto

# ---------------------------------------------------------------
Seccion "6. B3-02 Clientes"
$cl = T "6.1 Crear cliente" POST "/api/clientes" @{ primerNombre="Marta"; primerApellido="Zelaya"; telefono="8888-1000"; email="marta.$s@cliente.hn"; tipoCliente="Particular" } $tDesp @(200,201)
$clienteId = $cl.Body.id
$null = T "6.2 Sin telefono -> 400" POST "/api/clientes" @{ primerNombre="Sin"; primerApellido="Telefono"; tipoCliente="Particular" } $tDesp @(400)
$null = T "6.3 Email invalido -> 400" POST "/api/clientes" @{ primerNombre="Mal"; primerApellido="Correo"; telefono="8888-2000"; email="no-es-correo"; tipoCliente="Empresa" } $tDesp @(400)
$null = T "6.4 Segundo cliente (empresa)" POST "/api/clientes" @{ primerNombre="Ferreteria"; primerApellido="Central"; telefono="8888-3000"; tipoCliente="Empresa" } $tDesp @(200,201)
$b1 = T "6.5 Buscar por nombre (MARTA)" GET "/api/clientes?buscar=MARTA" $null $tDesp @(200)
Verificar "6.5b encuentra 1" ((Cuenta $b1.Body) -eq 1) $b1.Texto
$b2 = T "6.6 Buscar por telefono" GET "/api/clientes?buscar=8888-3" $null $tDesp @(200)
Verificar "6.6b encuentra la Ferreteria" ((Cuenta $b2.Body) -eq 1) $b2.Texto
$null = T "6.7 Editar cliente" PUT "/api/clientes/$clienteId" @{ primerNombre="Marta"; segundoNombre="Elena"; primerApellido="Zelaya"; telefono="8888-1001"; email="marta.$s@cliente.hn"; tipoCliente="Particular" } $tDesp @(200)
$lb = T "6.8 B lista clientes (vacio)" GET "/api/clientes" $null $tB @(200)
Verificar "6.8b B no ve clientes de A" ((Cuenta $lb.Body) -eq 0) $lb.Texto
$null = T "6.9 B edita cliente de A -> 404" PUT "/api/clientes/$clienteId" @{ primerNombre="X"; primerApellido="Y"; telefono="1"; tipoCliente="Particular" } $tB @(404)

# ---------------------------------------------------------------
Seccion "7. B3-03 Direcciones"
$da1 = T "7.1 Direccion origen de A" POST "/api/direcciones" @{ clienteId=$clienteId; ciudadId=$ciudadTegus; callePrincipal="Blvd Morazan"; numeroExterior="100"; coloniaSector="Palmira"; referencia="Frente al banco"; lat=14.0818; lng=-87.2068 } $tDesp @(200,201)
$dirA1 = $da1.Body.id
$da2 = T "7.2 Direccion destino de A" POST "/api/direcciones" @{ ciudadId=$ciudadSPS; callePrincipal="3a Avenida"; coloniaSector="Centro"; lat=15.5042; lng=-88.0250 } $tDesp @(200,201)
$dirA2 = $da2.Body.id
$null = T "7.3 Latitud fuera de rango -> 400" POST "/api/direcciones" @{ ciudadId=$ciudadTegus; callePrincipal="Calle X"; lat=95; lng=-87 } $tDesp @(400)
$null = T "7.4 Sin coordenadas -> 400" POST "/api/direcciones" @{ ciudadId=$ciudadTegus; callePrincipal="Sin coordenadas" } $tDesp @(400)
$null = T "7.5 clienteId de otra empresa -> 400/404" POST "/api/direcciones" @{ clienteId=$clienteId; ciudadId=$ciudadTegus; callePrincipal="Calle"; lat=14.0; lng=-87.0 } $tB @(400,404)
$ld = T "7.6 Listar direcciones de A" GET "/api/direcciones" $null $tDesp @(200)
Verificar "7.6b hay 2" ((Cuenta $ld.Body) -eq 2) $ld.Texto
$lf = T "7.7 Filtrar por cliente" GET "/api/direcciones?clienteId=$clienteId" $null $tDesp @(200)
Verificar "7.7b hay 1" ((Cuenta $lf.Body) -eq 1) $lf.Texto
$null = T "7.8 Editar direccion" PUT "/api/direcciones/$dirA2" @{ ciudadId=$ciudadSPS; callePrincipal="3a Avenida Norte"; coloniaSector="Centro"; lat=15.5042; lng=-88.0250 } $tDesp @(200)
$null = T "7.9 B edita direccion de A -> 404" PUT "/api/direcciones/$dirA2" @{ ciudadId=$ciudadSPS; callePrincipal="X"; lat=15.0; lng=-88.0 } $tB @(404)
$dc1 = T "7.10 Contratante C crea direccion origen" POST "/api/direcciones" @{ ciudadId=$ciudadTegus; callePrincipal="Col Kennedy bodega 5"; lat=14.1; lng=-87.2 } $tC @(200,201); $dirC1 = $dc1.Body.id
$dc2 = T "7.11 Contratante C crea direccion destino" POST "/api/direcciones" @{ ciudadId=$ciudadSPS; callePrincipal="Obra Torre Norte"; lat=15.5; lng=-88.0 } $tC @(200,201); $dirC2 = $dc2.Body.id

# ---------------------------------------------------------------
Seccion "8. B2-01 a B2-04 Flota, documentos y asignaciones"
$tv = T "8.1 Tipos de vehiculo" GET "/api/catalogos/tipos-vehiculo" $null $tDesp @(200)
Verificar "8.1b hay al menos 1 tipo" ((Cuenta $tv.Body) -ge 1) $tv.Texto
$v1 = T "8.2 Crear vehiculo 1" POST "/api/vehiculos" @{ tipoVehiculoId=$tipoCamion; placa="HAB-1001"; vin="VIN0000000000001"; propiedad="Propio" } $tDesp @(200,201); $veh1 = $v1.Body.id
Verificar "8.2b nace sin seguro vigente" ($v1.Body.seguroVigente -eq $false) $v1.Texto
$v2 = T "8.3 Crear vehiculo 2" POST "/api/vehiculos" @{ tipoVehiculoId=$tipoCamion; placa="HAB-1002"; propiedad="Externo" } $tDesp @(200,201); $veh2 = $v2.Body.id
$null = T "8.4 Placa repetida -> 409" POST "/api/vehiculos" @{ tipoVehiculoId=$tipoCamion; placa="HAB-1001"; propiedad="Propio" } $tDesp @(409)
$null = T "8.5 Tipo inexistente -> 400/404" POST "/api/vehiculos" @{ tipoVehiculoId="00000000-0000-0000-0000-000000000000"; placa="HAB-9999"; propiedad="Propio" } $tDesp @(400,404)
$null = T "8.6 B usa la misma placa en su empresa" POST "/api/vehiculos" @{ tipoVehiculoId=$tipoCamion; placa="HAB-1001"; propiedad="Propio" } $tB @(200,201)
$lv1 = T "8.7 Listar vehiculos de A" GET "/api/vehiculos" $null $tDesp @(200)
Verificar "8.7b hay 2" ((Cuenta $lv1.Body) -eq 2) $lv1.Texto
$null = T "8.8 Filtrar por estado Disponible" GET "/api/vehiculos?estado=Disponible" $null $tDesp @(200)
$null = T "8.9 Detalle vehiculo 1" GET "/api/vehiculos/$veh1" $null $tDesp @(200)
$null = T "8.10 B pide vehiculo de A -> 404" GET "/api/vehiculos/$veh1" $null $tB @(404)
$null = T "8.11 Vehiculo 2 a Mantenimiento" PUT "/api/vehiculos/$veh2" @{ estado="Mantenimiento"; vin="VIN0000000000002" } $tDesp @(200)
$null = T "8.12 Vehiculo 2 a Disponible" PUT "/api/vehiculos/$veh2" @{ estado="Disponible" } $tDesp @(200)

$null = T "8.13 Seguro vigente al vehiculo 1 (Admin)" POST "/api/vehiculos/$veh1/documentos" @{ tipo="Seguro"; numero="SEG-001"; fechaVencimiento=(Dia 365) } $tA @(200,201)
$null = T "8.14 Revision que vence en 10 dias" POST "/api/vehiculos/$veh1/documentos" @{ tipo="Revision"; numero="REV-001"; fechaVencimiento=(Dia 10) } $tA @(200,201)
$null = T "8.15 Revision ya vencida (vehiculo 2)" POST "/api/vehiculos/$veh2/documentos" @{ tipo="Revision"; numero="REV-002"; fechaVencimiento=(Dia -3) } $tA @(200,201)
$null = T "8.16 Despachador crea documento -> 403" POST "/api/vehiculos/$veh1/documentos" @{ tipo="Seguro"; numero="X"; fechaVencimiento=(Dia 30) } $tDesp @(403)
$null = T "8.17 B crea documento en vehiculo de A -> 404" POST "/api/vehiculos/$veh1/documentos" @{ tipo="Seguro"; numero="X"; fechaVencimiento=(Dia 30) } $tB @(404)
$dl = T "8.18 Listar documentos del vehiculo 1" GET "/api/vehiculos/$veh1/documentos" $null $tDesp @(200)
Verificar "8.18b hay 2" ((Cuenta $dl.Body) -eq 2) $dl.Texto
$vv = T "8.19 Vehiculo 1 con seguro vigente" GET "/api/vehiculos/$veh1" $null $tDesp @(200)
Verificar "8.19b seguroVigente=true" ($vv.Body.seguroVigente -eq $true) $vv.Texto
$pv = T "8.20 Documentos por vencer (30 dias)" GET "/api/documentos/por-vencer?dias=30" $null $tDesp @(200)
Verificar "8.20b trae 2 (REV-001 y REV-002 vencida)" ((Cuenta $pv.Body) -eq 2 -and ([regex]::Matches($pv.Texto, '"vencido":true')).Count -eq 1) $pv.Texto

$as1 = T "8.21 Asignar vehiculo 1 al conductor interno" POST "/api/asignaciones" @{ vehiculoId=$veh1; conductorId=$cond1Id } $tDesp @(200,201)
Verificar "8.21b nace vigente" ($as1.Body.vigente -eq $true) $as1.Texto
$as2 = T "8.22 Reasignar vehiculo 1 al conductor externo" POST "/api/asignaciones" @{ vehiculoId=$veh1; conductorId=$cond2Id } $tDesp @(200,201); $asig2 = $as2.Body.id
$hist = T "8.23 Historial del vehiculo 1" GET "/api/vehiculos/$veh1/asignaciones" $null $tDesp @(200)
Verificar "8.23b 2 filas y solo 1 vigente" ((Cuenta $hist.Body) -eq 2 -and ([regex]::Matches($hist.Texto, '"vigente":true')).Count -eq 1) $hist.Texto
$null = T "8.24 Historial del conductor (endpoint que falta en B2-04) -> 404 hoy" GET "/api/conductores/$cond1Id/asignaciones" $null $tDesp @(404)
$null = T "8.25 Finalizar asignacion vigente" PATCH "/api/asignaciones/$asig2/finalizar" $null $tDesp @(200,204)
$null = T "8.26 Finalizarla otra vez -> error" PATCH "/api/asignaciones/$asig2/finalizar" $null $tDesp @(400,404,409)
$null = T "8.27 B asigna vehiculo de A -> 400/404" POST "/api/asignaciones" @{ vehiculoId=$veh1; conductorId=$cond1Id } $tB @(400,404)
$null = T "8.28 Conductor externo a Inactivo" PUT "/api/conductores/$cond2Id" @{ primerNombre="Luis"; primerApellido="Mendoza"; licencia="LIC-0002"; venceLicencia=(Dia 15); estado="Inactivo" } $tDesp @(200)
$null = T "8.29 Asignar conductor Inactivo -> error" POST "/api/asignaciones" @{ vehiculoId=$veh2; conductorId=$cond2Id } $tDesp @(400,409)
$null = T "8.30 Conductor externo de nuevo Activo" PUT "/api/conductores/$cond2Id" @{ primerNombre="Luis"; primerApellido="Mendoza"; licencia="LIC-0002"; venceLicencia=(Dia 15); estado="Activo" } $tDesp @(200)
$null = T "8.31 Dejar asignacion vigente (vehiculo 1 al interno)" POST "/api/asignaciones" @{ vehiculoId=$veh1; conductorId=$cond1Id } $tDesp @(200,201)

# ---------------------------------------------------------------
Seccion "(Datos) Creando 3 envios de prueba directamente en la base (B4 aun no existe)"
$eComp = [guid]::NewGuid().ToString(); $ePend = [guid]::NewGuid().ToString(); $eAsig = [guid]::NewGuid().ToString()
$sqlEnv = @"
INSERT INTO envios (id, empresa_id, cliente_id, origen_direccion_id, destino_direccion_id, vehiculo_id, estado, tipo_servicio, peso_kg, fecha_programada, tarifa, costo_externo)
SELECT v.id, e.id, '$clienteId', '$dirA1', '$dirA2', v.vehiculo_id, v.estado, 'Carga', 1000, now() + interval '1 day', v.tarifa, 0
FROM empresas e JOIN usuarios u ON u.empresa_id = e.id AND u.email = '$emailA'
CROSS JOIN (VALUES
 ('$eComp'::uuid, NULL::uuid, 'Completado', 5000::numeric),
 ('$ePend'::uuid, NULL::uuid, 'Pendiente', 1000::numeric),
 ('$eAsig'::uuid, '$veh2'::uuid, 'Asignado', 2000::numeric)) AS v(id, vehiculo_id, estado, tarifa);
"@
if ($clienteId -and $dirA1 -and $dirA2 -and $veh2) { $r = Psql $sqlEnv } else { $r = "faltan clienteId/direcciones/vehiculo (fallaron pasos anteriores)"; $LASTEXITCODE = 1 }
Verificar "envios de prueba insertados" ($LASTEXITCODE -eq 0 -and ($r | Out-String) -match "INSERT 0 3") ($r | Out-String)

# ---------------------------------------------------------------
Seccion "9. B2-02 Baja de vehiculos"
$null = T "9.1 Baja del vehiculo 2 con envio Asignado -> 409" PATCH "/api/vehiculos/$veh2/baja" $null $tA @(409)
$null = T "9.2 Despachador da de baja -> 403" PATCH "/api/vehiculos/$veh1/baja" $null $tDesp @(403)
$null = T "9.3 Baja del vehiculo 1 (sin envios activos)" PATCH "/api/vehiculos/$veh1/baja" $null $tA @(200,204)
$h2 = T "9.4 Historial: la asignacion vigente se cerro" GET "/api/vehiculos/$veh1/asignaciones" $null $tDesp @(200)
Verificar "9.4b ninguna asignacion queda vigente" (([regex]::Matches($h2.Texto, '"vigente":true')).Count -eq 0) $h2.Texto
$lb1 = T "9.5 La lista oculta vehiculos en Baja" GET "/api/vehiculos" $null $tDesp @(200)
Verificar "9.5b solo queda 1 (HAB-1002)" ((Cuenta $lb1.Body) -eq 1) $lb1.Texto
$lb2 = T "9.6 Incluyendo Baja" GET "/api/vehiculos?incluirBaja=true" $null $tDesp @(200)
Verificar "9.6b salen 2" ((Cuenta $lb2.Body) -eq 2) $lb2.Texto

# ---------------------------------------------------------------
Seccion "10. B3-04 Facturas y pagos"
$f = T "10.1 Facturar envio Completado (tarifa 5000)" POST "/api/facturas" @{ envioId=$eComp; monedaCodigo="USD" } $tA @(200,201)
$facId = $f.Body.id
Verificar "10.1b monto=5000 y estado Pendiente" ($f.Body.monto -eq 5000 -and $f.Body.estado -eq "Pendiente") $f.Texto
$null = T "10.2 Facturar el mismo envio -> 409" POST "/api/facturas" @{ envioId=$eComp } $tA @(409)
$null = T "10.3 Facturar envio Pendiente -> 409" POST "/api/facturas" @{ envioId=$ePend } $tA @(409)
$null = T "10.4 B factura envio de A -> 404" POST "/api/facturas" @{ envioId=$eComp } $tB @(404)
$null = T "10.5 Despachador crea factura -> 403" POST "/api/facturas" @{ envioId=$eComp } $tDesp @(403)
$null = T "10.6 Despachador consulta facturas" GET "/api/facturas" $null $tDesp @(200)
$fp = T "10.7 Filtrar por estado Pendiente" GET "/api/facturas?estado=Pendiente" $null $tA @(200)
Verificar "10.7b hay 1" ((Cuenta $fp.Body) -eq 1) $fp.Texto
$fd = T "10.8 Detalle de la factura" GET "/api/facturas/$facId" $null $tA @(200)
Verificar "10.8b saldoPendiente=5000" ($fd.Body.saldoPendiente -eq 5000) $fd.Texto
$p1 = T "10.9 Pago parcial 2000 efectivo" POST "/api/facturas/$facId/pagos" @{ metodo="Efectivo"; monto=2000 } $tA @(200,201)
Verificar "10.9b saldoRestante=3000" ($p1.Body.saldoPendiente -eq 3000) $p1.Texto
$null = T "10.10 Pago mayor al saldo -> error" POST "/api/facturas/$facId/pagos" @{ metodo="Transferencia"; monto=4000 } $tA @(400,409)
$null = T "10.11 Pago con monto 0 -> 400" POST "/api/facturas/$facId/pagos" @{ metodo="Tarjeta"; monto=0 } $tA @(400)
$null = T "10.12 Metodo invalido -> 400" POST "/api/facturas/$facId/pagos" @{ metodo="Bitcoin"; monto=100 } $tA @(400)
$p2 = T "10.13 Pago final 3000 transferencia" POST "/api/facturas/$facId/pagos" @{ metodo="Transferencia"; monto=3000 } $tA @(200,201)
Verificar "10.13b factura Pagada y saldo 0" ($p2.Body.saldoPendiente -eq 0 -and $p2.Body.estado -eq "Pagada") $p2.Texto
$null = T "10.14 Pagar factura ya Pagada -> 409" POST "/api/facturas/$facId/pagos" @{ metodo="Efectivo"; monto=1 } $tA @(400,409)
$fg = T "10.15 Filtrar por estado Pagada" GET "/api/facturas?estado=Pagada" $null $tA @(200)
Verificar "10.15b hay 1" ((Cuenta $fg.Body) -eq 1) $fg.Texto
$null = T "10.16 B consulta factura de A -> 404" GET "/api/facturas/$facId" $null $tB @(404)

# ---------------------------------------------------------------
Seccion "11. B5-01 y B5-02 Subastas"
$sub = @{ tipoVehiculoId=$tipoCamion; origenDireccionId=$dirC1; destinoDireccionId=$dirC2; descripcion="20 toneladas de cemento"; pesoKg=20000; volumenM3=15; precioMaximo=25000; monedaCodigo="HNL"; fechaCierre=(Iso 30) }
$sc = T "11.1 Publicar subasta" POST "/api/subastas" $sub $tC2 @(200,201); $subId = $sc.Body.id
Verificar "11.1b estado Abierta" ($sc.Body.estado -eq "Abierta") $sc.Texto
$null = T "11.2 Transportista publica -> 403" POST "/api/subastas" $sub $tA @(403)
$x = $sub.Clone(); $x.fechaCierre = (Iso -1)
$null = T "11.3 Cierre en el pasado -> 400" POST "/api/subastas" $x $tC2 @(400)
$x = $sub.Clone(); $x.destinoDireccionId = $dirC1
$null = T "11.4 Origen igual al destino -> 400" POST "/api/subastas" $x $tC2 @(400)
$x = $sub.Clone(); $x.origenDireccionId = $dirA1; $x.destinoDireccionId = $dirA2
$null = T "11.5 Direcciones de otra empresa -> 400/404" POST "/api/subastas" $x $tC2 @(400,404)
$x = $sub.Clone(); $x.monedaCodigo = "XXX"
$null = T "11.6 Moneda inexistente -> 400" POST "/api/subastas" $x $tC2 @(400)
$mi = T "11.7 Mis subastas" GET "/api/subastas/mias" $null $tC2 @(200)
Verificar "11.7b hay 1" ((Cuenta $mi.Body) -eq 1) $mi.Texto
$mf = T "11.8 Filtrar por estado Abierta" GET "/api/subastas/mias?estado=Abierta" $null $tC2 @(200)
Verificar "11.8b hay 1" ((Cuenta $mf.Body) -eq 1) $mf.Texto
$sd = T "11.9 Detalle de la subasta" GET "/api/subastas/$subId" $null $tC2 @(200)
Verificar "11.9b totalPujas=0" ($sd.Body.totalPujas -eq 0) $sd.Texto
$null = T "11.10 D pide subasta de C -> 404" GET "/api/subastas/$subId" $null $tD2 @(404)
$md = T "11.11 D lista sus subastas (vacio)" GET "/api/subastas/mias" $null $tD2 @(200)
Verificar "11.11b D no ve las de C" ((Cuenta $md.Body) -eq 0) $md.Texto
$null = T "11.12 D cancela subasta de C -> 404" PATCH "/api/subastas/$subId/cancelar" $null $tD2 @(404)
$cc = T "11.13 C cancela su subasta" PATCH "/api/subastas/$subId/cancelar" $null $tC2 @(200,204)
$null = T "11.14 Cancelar otra vez -> 409" PATCH "/api/subastas/$subId/cancelar" $null $tC2 @(409)
$sh = $sub.Clone(); $sh.fechaCierre = (Get-Date).ToUniversalTime().AddSeconds(45).ToString("yyyy-MM-ddTHH:mm:ssZ")
$sk = T "11.15 Subasta que cierra en 45 segundos" POST "/api/subastas" $sh $tC2 @(200,201); $subCorta = $sk.Body.id
if (-not $SinEsperaCierre) {
    Write-Host "  ... esperando 50 segundos para probar el cierre automatico ..." -ForegroundColor Yellow
    Start-Sleep -Seconds 50
    $sv = T "11.16 Consultar tras el cierre" GET "/api/subastas/$subCorta" $null $tC2 @(200)
    Verificar "11.16b estado Vencida (cierre automatico)" ($sv.Body.estado -eq "Vencida") $sv.Texto
}

# ---------------------------------------------------------------
Write-Host ""
Write-Host "================ RESUMEN ================" -ForegroundColor Cyan
Write-Host ("PASS: " + $script:pass) -ForegroundColor Green
Write-Host ("FAIL: " + $script:fail) -ForegroundColor $(if ($script:fail -eq 0) { "Green" } else { "Red" })
if ($script:fallos.Count -gt 0) {
    Write-Host ""; Write-Host "Pruebas que fallaron:" -ForegroundColor Red
    $script:fallos | ForEach-Object { Write-Host (" - " + $_) -ForegroundColor Red }
}
Write-Host ""
Write-Host "No existen todavia (no se prueban): B2-05 Docker, B3-05 CI/CD, B4-01..B4-05 Envios, B5-03..B5-05 Pujas/Adjudicar." -ForegroundColor DarkYellow

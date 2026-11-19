using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace quickgo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ciudades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    codigo_postal = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciudades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    primer_nombre = table.Column<string>(type: "text", nullable: false),
                    segundo_nombre = table.Column<string>(type: "text", nullable: true),
                    primer_apellido = table.Column<string>(type: "text", nullable: false),
                    segundo_apellido = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    tipo_cliente = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cotizaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto = table.Column<decimal>(type: "numeric", nullable: false),
                    moneda_codigo = table.Column<string>(type: "text", nullable: false),
                    valida_hasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    aceptada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotizaciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departamentos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_comercial = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    razon_social = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rtn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    plan = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto = table.Column<decimal>(type: "numeric", nullable: false),
                    moneda_codigo = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "monedas",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    simbolo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monedas", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "paises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    codigo_iso = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seguimiento_envio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lat = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: false),
                    lng = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    momento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seguimiento_envio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_vehiculo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    capacidad_kg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    largo_m = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ancho_m = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    alto_m = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    volumen_max_m3 = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_vehiculo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "direcciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ciudad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calle_principal = table.Column<string>(type: "text", nullable: false),
                    numero_exterior = table.Column<string>(type: "text", nullable: true),
                    colonia_sector = table.Column<string>(type: "text", nullable: true),
                    referencia = table.Column<string>(type: "text", nullable: true),
                    lat = table.Column<decimal>(type: "numeric", nullable: false),
                    lng = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_direcciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_direcciones_ciudades_ciudad_id",
                        column: x => x.ciudad_id,
                        principalTable: "ciudades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metodo = table.Column<string>(type: "text", nullable: false),
                    monto = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pagos_facturas_factura_id",
                        column: x => x.factura_id,
                        principalTable: "facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    primer_nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    segundo_nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    primer_apellido = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    segundo_apellido = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehiculos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    placa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    propiedad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehiculos", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehiculos_tipos_vehiculo_tipo_vehiculo_id",
                        column: x => x.tipo_vehiculo_id,
                        principalTable: "tipos_vehiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subastas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen_direccion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino_direccion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    peso_kg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    volumen_m3 = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    precio_maximo = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    moneda_codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fecha_cierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subastas", x => x.id);
                    table.ForeignKey(
                        name: "FK_subastas_direcciones_destino_direccion_id",
                        column: x => x.destino_direccion_id,
                        principalTable: "direcciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subastas_direcciones_origen_direccion_id",
                        column: x => x.origen_direccion_id,
                        principalTable: "direcciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subastas_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subastas_monedas_moneda_codigo",
                        column: x => x.moneda_codigo,
                        principalTable: "monedas",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subastas_tipos_vehiculo_tipo_vehiculo_id",
                        column: x => x.tipo_vehiculo_id,
                        principalTable: "tipos_vehiculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conductores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_conductor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    licencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vence_licencia = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conductores", x => x.id);
                    table.ForeignKey(
                        name: "FK_conductores_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conductores_usuarios_id",
                        column: x => x.id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documentos_vehiculo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    numero = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos_vehiculo", x => x.id);
                    table.ForeignKey(
                        name: "FK_documentos_vehiculo_vehiculos_vehiculo_id",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "envios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subasta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origen_direccion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino_direccion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conductor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_servicio = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    peso_kg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    volumen_m3 = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    fecha_programada = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tarifa = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    costo_externo = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envios", x => x.id);
                    table.ForeignKey(
                        name: "FK_envios_vehiculos_vehiculo_id",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pujas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subasta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conductor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pujas", x => x.id);
                    table.ForeignKey(
                        name: "FK_pujas_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pujas_subastas_subasta_id",
                        column: x => x.subasta_id,
                        principalTable: "subastas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asignaciones_vehiculo_conductor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conductor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaciones_vehiculo_conductor", x => x.id);
                    table.ForeignKey(
                        name: "FK_asignaciones_vehiculo_conductor_conductores_conductor_id",
                        column: x => x.conductor_id,
                        principalTable: "conductores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignaciones_vehiculo_conductor_vehiculos_vehiculo_id",
                        column: x => x.vehiculo_id,
                        principalTable: "vehiculos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "descripcion", "nombre" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), "Administra la empresa, sus usuarios y la facturación", "Admin" },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), "Gestiona envios, flota y conductores", "Despachador" },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), "Ejecuta los envios asignados", "Conductor" },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), "Publica subastas de transporte", "Contratante" }
                });

            migrationBuilder.InsertData(
                table: "tipos_vehiculo",
                columns: new[] { "id", "alto_m", "ancho_m", "capacidad_kg", "largo_m", "nombre", "volumen_max_m3" },
                values: new object[,]
                {
                    { new Guid("a23fa570-2f4b-4f85-80e9-042d0d315001"), 3m, 2.5m, 12000m, 8m, "Camión de carga", 60m },
                    { new Guid("a23fa570-2f4b-4f85-80e9-042d0d315002"), 1m, 2.5m, 15000m, 10m, "Plataforma", 25m },
                    { new Guid("a23fa570-2f4b-4f85-80e9-042d0d315003"), 2.2m, 1.2m, 3000m, 2.5m, "Montacargas", 6.6m },
                    { new Guid("a23fa570-2f4b-4f85-80e9-042d0d315004"), 3.2m, 2.5m, 20000m, 7m, "Excavadora", 56m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_vehiculo_conductor_conductor_id",
                table: "asignaciones_vehiculo_conductor",
                column: "conductor_id");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_vehiculo_conductor_vehiculo_id",
                table: "asignaciones_vehiculo_conductor",
                column: "vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "ux_conductores_empresa_licencia",
                table: "conductores",
                columns: new[] { "empresa_id", "licencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_direcciones_ciudad_id",
                table: "direcciones",
                column: "ciudad_id");

            migrationBuilder.CreateIndex(
                name: "IX_documentos_vehiculo_vehiculo_id",
                table: "documentos_vehiculo",
                column: "vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "ux_empresas_rtn",
                table: "empresas",
                column: "rtn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_empresas_slug",
                table: "empresas",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_envios_vehiculo_id",
                table: "envios",
                column: "vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_factura_id",
                table: "pagos",
                column: "factura_id");

            migrationBuilder.CreateIndex(
                name: "IX_pujas_empresa_id",
                table: "pujas",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_pujas_subasta_id_empresa_id",
                table: "pujas",
                columns: new[] { "subasta_id", "empresa_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_roles_nombre",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subastas_destino_direccion_id",
                table: "subastas",
                column: "destino_direccion_id");

            migrationBuilder.CreateIndex(
                name: "IX_subastas_empresa_id",
                table: "subastas",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_subastas_estado_fecha_cierre",
                table: "subastas",
                columns: new[] { "estado", "fecha_cierre" });

            migrationBuilder.CreateIndex(
                name: "IX_subastas_moneda_codigo",
                table: "subastas",
                column: "moneda_codigo");

            migrationBuilder.CreateIndex(
                name: "IX_subastas_origen_direccion_id",
                table: "subastas",
                column: "origen_direccion_id");

            migrationBuilder.CreateIndex(
                name: "IX_subastas_tipo_vehiculo_id",
                table: "subastas",
                column: "tipo_vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "IX_tipos_vehiculo_nombre",
                table: "tipos_vehiculo",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_empresa_id",
                table: "usuarios",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_rol_id",
                table: "usuarios",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "ux_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehiculos_tipo_vehiculo_id",
                table: "vehiculos",
                column: "tipo_vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "ux_vehiculos_empresa_placa",
                table: "vehiculos",
                columns: new[] { "empresa_id", "placa" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asignaciones_vehiculo_conductor");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "cotizaciones");

            migrationBuilder.DropTable(
                name: "departamentos");

            migrationBuilder.DropTable(
                name: "documentos_vehiculo");

            migrationBuilder.DropTable(
                name: "envios");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "paises");

            migrationBuilder.DropTable(
                name: "pujas");

            migrationBuilder.DropTable(
                name: "seguimiento_envio");

            migrationBuilder.DropTable(
                name: "conductores");

            migrationBuilder.DropTable(
                name: "vehiculos");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "subastas");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "direcciones");

            migrationBuilder.DropTable(
                name: "monedas");

            migrationBuilder.DropTable(
                name: "tipos_vehiculo");

            migrationBuilder.DropTable(
                name: "empresas");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "ciudades");
        }
    }
}

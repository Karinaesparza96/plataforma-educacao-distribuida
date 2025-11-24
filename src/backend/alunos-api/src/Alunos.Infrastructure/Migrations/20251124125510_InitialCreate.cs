using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alunos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alunos",
                columns: table => new
                {
                    AlunoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    CodigoUsuarioAutenticacao = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    Nome = table.Column<string>(type: "Varchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "Varchar(100)", maxLength: 100, nullable: false),
                    Cpf = table.Column<string>(type: "Varchar(11)", maxLength: 11, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "SmallDateTime", nullable: false),
                    Telefone = table.Column<string>(type: "Varchar(25)", maxLength: 25, nullable: true),
                    Ativo = table.Column<bool>(type: "Bit", nullable: false),
                    Genero = table.Column<string>(type: "Varchar(20)", maxLength: 20, nullable: false),
                    Cidade = table.Column<string>(type: "Varchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "Varchar(2)", maxLength: 2, nullable: true),
                    Cep = table.Column<string>(type: "Varchar(8)", maxLength: 8, nullable: false),
                    Foto = table.Column<string>(type: "Varchar(1024)", maxLength: 1024, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "DateTime", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "DateTime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AlunosPK", x => x.AlunoId);
                });

            migrationBuilder.CreateTable(
                name: "MatriculasCursos",
                columns: table => new
                {
                    MatriculaCursoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    AlunoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    CursoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    NomeCurso = table.Column<string>(type: "Varchar(200)", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "Money", precision: 10, scale: 2, nullable: false),
                    DataMatricula = table.Column<DateTime>(type: "SmallDateTime", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "SmallDateTime", nullable: true),
                    EstadoMatricula = table.Column<byte>(type: "TinyInt", nullable: false),
                    Observacao = table.Column<string>(type: "Varchar(2000)", maxLength: 2000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "DateTime", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "DateTime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("MatriculasCursosPK", x => x.MatriculaCursoId);
                    table.ForeignKey(
                        name: "MatriculasCursosAlunosFK",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "AlunoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificados",
                columns: table => new
                {
                    CertificadoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    MatriculaCursoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    NomeCurso = table.Column<string>(type: "Varchar(200)", maxLength: 200, nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "SmallDateTime", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "SmallDateTime", nullable: true),
                    CargaHoraria = table.Column<short>(type: "SmallInt", nullable: false),
                    NotaFinal = table.Column<byte>(type: "TinyInt", nullable: false),
                    PathCertificado = table.Column<string>(type: "Varchar(1024)", maxLength: 1024, nullable: false),
                    NomeInstrutor = table.Column<string>(type: "Varchar(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "DateTime", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "DateTime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CertificadosPK", x => x.CertificadoId);
                    table.ForeignKey(
                        name: "MatriculasCursosCertificadosFK",
                        column: x => x.MatriculaCursoId,
                        principalTable: "MatriculasCursos",
                        principalColumn: "MatriculaCursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosAprendizado",
                columns: table => new
                {
                    HistoricoAprendizadoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    MatriculaCursoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    CursoId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    AulaId = table.Column<Guid>(type: "UniqueIdentifier", nullable: false),
                    NomeAula = table.Column<string>(type: "Varchar(100)", maxLength: 100, nullable: false),
                    CargaHoraria = table.Column<int>(type: "Int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "SmallDateTime", nullable: false),
                    DataTermino = table.Column<DateTime>(type: "SmallDateTime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("HistoricoAprendizadoPK", x => x.HistoricoAprendizadoId);
                    table.ForeignKey(
                        name: "FK_HistoricosAprendizado_MatriculasCursos_MatriculaCursoId",
                        column: x => x.MatriculaCursoId,
                        principalTable: "MatriculasCursos",
                        principalColumn: "MatriculaCursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "AlunosEmailUK",
                table: "Alunos",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "AlunosNomeIDX",
                table: "Alunos",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "CertificadosMatriculaCursoIdIDX",
                table: "Certificados",
                column: "MatriculaCursoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "HistoricosAprendizadoAulaIdIDX",
                table: "HistoricosAprendizado",
                column: "AulaId");

            migrationBuilder.CreateIndex(
                name: "HistoricosAprendizadoCursoIdIDX",
                table: "HistoricosAprendizado",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosAprendizado_MatriculaCursoId",
                table: "HistoricosAprendizado",
                column: "MatriculaCursoId");

            migrationBuilder.CreateIndex(
                name: "MatriculasCursosAlunoIdIDX",
                table: "MatriculasCursos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "MatriculasCursosCursoIdIDX",
                table: "MatriculasCursos",
                column: "CursoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificados");

            migrationBuilder.DropTable(
                name: "HistoricosAprendizado");

            migrationBuilder.DropTable(
                name: "MatriculasCursos");

            migrationBuilder.DropTable(
                name: "Alunos");
        }
    }
}

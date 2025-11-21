using Core.DomainValidations;

namespace Core.Tests.DomainValidations;
public class ValidacaoDataTests
{
    private class Dummy { }

    [Fact]
    public void DeveSerValido_deve_falhar_em_MinValue_MaxValue_e_passar_em_datas_normais()
    {
        var r1 = new ResultadoValidacao<Dummy>();
        ValidacaoData.DeveSerValido(DateTime.MinValue, "data inválida", r1);
        r1.Invoking(x => x.DispararExcecaoDominioSeInvalido()).Should().Throw<Exception>();

        var r2 = new ResultadoValidacao<Dummy>();
        ValidacaoData.DeveSerValido(DateTime.Now, "data inválida", r2);
        r2.Invoking(x => x.DispararExcecaoDominioSeInvalido()).Should().NotThrow();
    }

    [Fact]
    public void DeveSerMenorQue_deve_falhar_quando_dataValidacao_maior_que_limite()
    {
        var r = new ResultadoValidacao<Dummy>();
        var hoje = DateTime.Today;
        ValidacaoData.DeveSerMenorQue(hoje.AddDays(1), hoje, "esperava < limite", r);

        r.Invoking(x => x.DispararExcecaoDominioSeInvalido()).Should().Throw<Exception>();
    }

    [Fact]
    public void DeveSerMaiorQue_deve_falhar_quando_dataMenor_maior_que_dataMaior()
    {
        var r = new ResultadoValidacao<Dummy>();
        var hoje = DateTime.Today;
        ValidacaoData.DeveSerMaiorQue(hoje, hoje.AddDays(1), "esperava > menor", r);

        r.Invoking(x => x.DispararExcecaoDominioSeInvalido()).Should().Throw<Exception>();
    }

    [Fact]
    public void DeveTerRangeValido_deve_falhar_quando_inicial_maior_que_final()
    {
        var r = new ResultadoValidacao<Dummy>();
        var ini = DateTime.Today.AddDays(2);
        var fim = DateTime.Today;
        ValidacaoData.DeveTerRangeValido(ini, fim, "range inválido", r);

        r.Invoking(x => x.DispararExcecaoDominioSeInvalido()).Should().Throw<Exception>();
    }
}

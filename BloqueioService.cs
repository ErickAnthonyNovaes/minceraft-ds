using System;
using System.Collections.Generic;

public class BloqueioService
{
    public bool EstaBloqueado { get; set; } = false;

    // Tokens curtos para liberar acesso temporário (opcional)
    private readonly Dictionary<string, DateTime> _tokens = new();

    public string GerarToken(int segundosValidade = 10)
    {
        var token = Guid.NewGuid().ToString("N");
        _tokens[token] = DateTime.UtcNow.AddSeconds(segundosValidade);
        return token;
    }

    public bool ValidarToken(string token)
    {
        if (_tokens.TryGetValue(token, out var exp) && exp > DateTime.UtcNow)
            return true;

        _tokens.Remove(token);
        return false;
    }
}
namespace GerenciadorDePedidos.Core.Services;

public interface ICacheService
{
	T? Get<T>(string key);
	void Set<T>(string key, T value, TimeSpan expiration);
	bool TryGet<T>(string key, out T? value);
	void Remove(string key);
	void RemoveMany(IEnumerable<string> keys);
	void RemoveByPrefix(string prefix);
}
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities
{
    public class Login : BaseEntity<int>
    {
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public Role Role { get; set; }

        public Login(string email, string senhaHash)
        {
            Email = email;
            SenhaHash = senhaHash;
        }
    }
}

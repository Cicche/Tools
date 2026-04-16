
using System.Collections;

namespace Tools

{
    public class PC
    {
        string _type = null;
        string _nome = null;
        string _ip = null;
        string _user = null;
        string _password = null;
        bool _pingable = false;

        public PC(string type, string nome, string ip, string user, string passw)
        {
            _type = type;
            _nome = nome;
            _ip = ip;
            _user = user;
            _password = passw;
        }

        public PC()
        {

        }

        public string Type { get => _type; set => _type = value; }
        public string Nome { get => _nome; set => _nome = value; }
        public string Ip { get => _ip; set => _ip = value; }
        public string User { get => _user; set => _user = value; }
        public string Password { get => _password; set => _password = value; }
        public bool Pingable { get => _pingable; set => _pingable = value; }

    }
}

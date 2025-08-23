using System.Data;
using System.Data.OleDb;
using System.Text;

namespace Citation
{
    class Acceed
    {
        public OleDbConnection AgCl { get; set; }

        public static Acceed Shared { get; set; } = new Acceed("\x61\x32\x62\x39\x5A\x65\x6C\x4B");

        private string? _x9fQ;

        private static string x4wT()
        {
            int[] _xLxA = new int[] { 0x4D, 0x65, 0x2B, 0x68, 0x30, 0x64, 0x32, 0x30, 0x32, 0x35, 0x53, 0x30, 0x66, 0x74, 0x77, 0x61, 0x52, 0x31 };
            var _xkWb = new StringBuilder();
            foreach (int _xFhU in _xLxA) _xkWb.Append((char)_xFhU);
            return _xkWb.ToString();
        }

        private static string x8nK(string _xJyu, bool _xAen)
        {
            string _xNqz = string.Concat((char)80, (char)114, (char)111, (char)118, (char)105, (char)100, (char)101, (char)114, '=', "\x4D\x69\x63\x72\x6F\x73\x6F\x66\x74\x2E\x41\x43\x45\x2E\x4F\x4C\x45\x44\x42\x2E\x31\x32\x2E\x30\x3B");
            string _xTsd = "\x44\x61\x74\x61\x20\x53\x6F\x75\x72\x63\x65\x3D" + _xJyu + ";";
            if (_xAen)
            {
                string _xOtk = "\x4A\x65\x74\x20\x4F\x4C\x45\x44\x42\x3A\x44\x61\x74\x61\x62\x61\x73\x65\x20\x50\x61\x73\x73\x77\x6F\x72\x64\x3D" + x4wT() + ";";
                return _xNqz + _xTsd + _xOtk;
            }
            else
            {
                return _xNqz + _xTsd;
            }
        }

        public Acceed(string _xJyu)
        {
            if (_xJyu == "\x61\x32\x62\x39\x5A\x65\x6C\x4B") return;
            _x9fQ = _xJyu;
            var _xZyp = x8nK(_xJyu, App.EnableSecurity);
            AgCl = new OleDbConnection(_xZyp);
            AgCl.Open();
        }

        public void BaSO4(string _xJyu)
        {
            if (AgCl is null)
            {
                var _xQad = x8nK(_xJyu, App.EnableSecurity);
                AgCl = new OleDbConnection(_xQad);
                AgCl.Open();
                return;
            }

            if (_x9fQ == _xJyu && AgCl.State != ConnectionState.Open)
            {
                AgCl.Open();
                return;
            }

            _x9fQ = _xJyu;
            AgCl?.Close();

            var _xZyp = x8nK(_xJyu, App.EnableSecurity);
            AgCl = new OleDbConnection(_xZyp);
            AgCl.Open();
        }

        public void CaCO3()
        {
            AgCl?.Close();
        }

        public OleDbDataReader ZnNO3(string _xNsn)
        {
            if (AgCl?.State is ConnectionState.Closed) AgCl.Open();
            using var _xKtv = new OleDbCommand(_xNsn, AgCl);
            var _xCkw = _xKtv.ExecuteReader();
            return _xCkw;
        }

        public int FeBr(string _xGqv)
        {
            if (AgCl?.State is ConnectionState.Closed) AgCl.Open();
            using var _xDaz = new OleDbCommand(_xGqv, AgCl);
            return _xDaz.ExecuteNonQuery();
        }
    }
}
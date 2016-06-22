using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection;

namespace FjeeaRefresher
{
    class Encryption
    {
        private static Type ScriptType = Type.GetTypeFromProgID("ScriptControl");
        private static object ScriptControl = Activator.CreateInstance(ScriptType);
        private static bool ScriptInitialized = false;
        private static Dictionary<string, string> CachedPassword = new Dictionary<string, string>();

        public static void InitializeScriptControl()
        {
            if (ScriptInitialized) return;
            ScriptInitialized = true;
            ScriptType.InvokeMember("Language", BindingFlags.SetProperty, null, ScriptControl, new object[] { "JScript" });
            ScriptType.InvokeMember("AddCode", BindingFlags.InvokeMethod, null, ScriptControl, new object[] { FjeeaResource.Properties.Resources.RSAJavaScript });
        }

        // Fuck Fjeea's RSA!
        public static string Encrypt(string Password)
        {
            if (CachedPassword.ContainsKey(Password))
            {
                return CachedPassword[Password];
            }
            InitializeScriptControl();
            string ret = ScriptType.InvokeMember("Run", System.Reflection.BindingFlags.InvokeMethod, null, ScriptControl, new object[] { "do_encrypt", Password }).ToString();
            return ret;
        }

    }
}

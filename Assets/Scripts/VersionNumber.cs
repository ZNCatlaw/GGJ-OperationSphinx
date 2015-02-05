using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

[assembly: AssemblyVersion("1.0.*.*")]
public class VersionNumber : MonoBehaviour
{
    public Text textBox;
    public bool hideAfter10Seconds = true;

    static System.Version aVersion = Assembly.GetExecutingAssembly().GetName().Version;
    string version;

    void Start()
    {
        version = aVersion.ToString();

        Debug.Log(string.Format("Currently running version is {0}", version));
        textBox.text = string.Format("v{0}", version);
    }

    void Update()
    {
        if (hideAfter10Seconds && Time.time > 10) {
            textBox.text = "";
            Destroy(this);
        }
    }
}
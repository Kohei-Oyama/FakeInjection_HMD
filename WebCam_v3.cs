using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

public class WebCam_v3 : MonoBehaviour {

    public int Width; // FULL HD: 1920*960/30
    public int Height;
    public int FPS;
    public int re_time; //録画時間
    public Material material;
    public Color32[][] data; //表示用のデータ格納配列
    public List<Color32[]> dataList; //表示用のデータ格納
    public Texture2D texture; //実際に録画したフレーム数
    WebCamTexture webcamTexture;
    [SerializeField] Camera target;

    // Use this for initialization
    void Start () {
        material = GetTargetMaterial();
        dataList = new List<Color32[]>();
        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
        {
            string camname = devices[i].name;

            // Logicool HD Pro Webcam C910
            // RICOH THETA V FullHD
            // RICOH THETA S
            if (camname == "RICOH THETA S") 
            {
                webcamTexture = new WebCamTexture(camname, Width, Height, FPS);
                material.mainTexture = webcamTexture;
                webcamTexture.Play();

                // 録画フレームは多めに確保
                data = new Color32[FPS * re_time * 5][];
                for (var j = 0; j < FPS * re_time * 5; j++)
                {
                    data[j] = new Color32[webcamTexture.width * webcamTexture.height];
                }
                texture = new Texture2D(webcamTexture.width, webcamTexture.height);

                break;
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (webcamTexture != null)
            {
                this.StartCoroutine(this.recordTHEATA());
            }
        } else if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // 録画と逆再生
    private IEnumerator recordTHEATA()
    {
        //print("Start Recording!!");
        float startTime = Time.time;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch();

        sw.Start();

        var wait = new WaitForSeconds(0.1f);
        int recordFrame = 0; //実際に録画したフレーム数
        while (Time.time - startTime < re_time)
        {
            webcamTexture.GetPixels32(data[recordFrame]);

            dataList.Add(data[recordFrame]);
            recordFrame += 1;
            yield return wait;
        }
        sw.Stop();

        print("End Recording!!");
        print("Start Reverse!!");

        dataList.Reverse();

        float sleepFrame = ((float)re_time / recordFrame) - (float)0.015;
        var wait2 = new WaitForSeconds(sleepFrame);

        sw2.Start();
        for (int i = 0; i < recordFrame; i++)
        {
            texture.SetPixels32(dataList[i]);
            material.mainTexture = texture;
            texture.Apply();
            yield return wait2;
        }
        sw2.Stop();

        print("End Reverse!!");
        yield return null;
    }

    Material GetTargetMaterial()
    {
        Skybox skybox = GetComponent<Skybox>();
        if (skybox != null)
        {
            return skybox.material;
        }
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.material;
        }
        Debug.LogError("Renderer/Skyboxコンポーネントがありません。");
        return null;
    }

}

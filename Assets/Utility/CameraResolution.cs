using UnityEngine;
using System.Collections;

public class CameraResolution : MonoBehaviour {
    public static readonly float PIXELS_PER_UNIT = 32;
    public GameObject background;

    public void Start() {
        Camera.main.orthographicSize = (Screen.height / PIXELS_PER_UNIT) * 0.5f;
    }

    public void Update() {       
        if(Input.GetKey(KeyCode.LeftAlt) && Input.GetAxis("Mouse ScrollWheel") < 0) {
            Camera.main.orthographicSize *= 2;
        }
        if(Input.GetKey(KeyCode.LeftAlt) && Input.GetAxis("Mouse ScrollWheel") > 0) {
            Camera.main.orthographicSize /= 2;
        }

        //SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        //background.transform.localScale = new Vector3(10, 10, 1);

        //float width = sr.sprite.bounds.size.x;
        //float height = sr.sprite.bounds.size.y;

        //float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        //float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        //background.transform.localScale = new Vector3(worldScreenWidth / width, worldScreenWidth / width, 1);
    }
}

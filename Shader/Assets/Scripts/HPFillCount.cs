using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPFillCount : MonoBehaviour {

    private Image image;

    private Image enemyHpImage;

    private Material enemyHpMat;

    public GameObject targetGameObject;

    private MaterialPropertyBlock propertyBlock;
    void Start () {
        image = this.GetComponent<Image>();
        enemyHpImage = targetGameObject.GetComponent<Image>();
        enemyHpMat = enemyHpImage.material;

        // propertyBlock = new MaterialPropertyBlock();

        // Material m = Resources.Load<Material>("EnimyHP");
        // enemyHpImage.material = m;

    }

	// Update is called once per frame
	void Update () {
        float c = image.fillAmount;
        enemyHpMat.SetFloat("_fillCount", c);

        Color color = enemyHpImage.color;
        enemyHpMat.SetColor("_Color", color);

        //targetGameObject.GetComponent<Renderer>().GetPropertyBlock(propertyBlock);
        // propertyBlock.SetFloat("_fillCount", c);
        // targetGameObject.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
        //  listProp[i].GetComponent<Renderer>().GetPropertyBlock(prop);
        //         prop.SetColor(colorID, new Color(r, g, b, 1));
        //         listProp[i].GetComponent<Renderer>().SetPropertyBlock(prop);

	}
}

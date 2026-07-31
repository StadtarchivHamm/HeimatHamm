using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CutoutMaskImage : Image
{
    public override Material materialForRendering
    {
        get 
        {
            Material material = new Material(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }

    protected override void OnEnable() 
    {
        base.OnEnable();
        StartCoroutine(SetMaterialDirtyWithDelay());
    }


    private IEnumerator SetMaterialDirtyWithDelay()
    {
        yield return null;
        SetMaterialDirty();
    }
}

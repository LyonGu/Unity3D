using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


/*
    BeforeTransparent: the effect will only be applied to opaque objects before the transparent pass is done.
    BeforeStack: the effect will be applied before the built-in stack kicks-in. That includes anti-aliasing, depth-of-field, tonemapping etc.
    AfterStack: the effect will be applied after the builtin stack and before FXAA (if it's enabled) & final-pass dithering.
     
 */


// Settings
[Serializable]
[PostProcess(typeof(GrayscaleRenderer), PostProcessEvent.AfterStack, "Custom/Grayscale")]
public sealed  class GrayScale : PostProcessEffectSettings
{

    [Range(0f, 1f), Tooltip("Grayscale effect intensity.")]
    public FloatParameter blend = new FloatParameter { value = 0.5f };

    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value
            && blend.value > 0f;
    }
}


//Renderer
public sealed class GrayscaleRenderer : PostProcessEffectRenderer<GrayScale>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Grayscale"));
        sheet.properties.SetFloat("_Blend", settings.blend);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}

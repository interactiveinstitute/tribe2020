#ifndef _TRAMPOLINE_UNITY_CUSTOM_CMVIDEOSAMPLING_H_
#define _TRAMPOLINE_UNITY_CUSTOM_CMVIDEOSAMPLING_H_

// small helper for getting texture from CMSampleBuffer
// uses CVOpenGLESTextureCache if available and falls back to simple copy otherwise

typedef struct
CustomCMVideoSampling
{
    // CVOpenGLESTextureCache support
    void*   cvTextureCache;
    void*   cvTextureCacheTexture;

    // double-buffered pixel read if no CVOpenGLESTextureCache support
    int     glTex[2];
}
CustomCMVideoSampling;

void CustomCMVideoSampling_Initialize(CustomCMVideoSampling* sampling);
void CustomCMVideoSampling_Uninitialize(CustomCMVideoSampling* sampling);

// buffer is CMSampleBufferRef
// returns gltex id
int  CustomCMVideoSampling_SampleBuffer(CustomCMVideoSampling* sampling, void* buffer, int w, int h);
int  CustomCMVideoSampling_LastSampledTexture(CustomCMVideoSampling* sampling);


#endif // _TRAMPOLINE_UNITY_CMVIDEOSAMPLING_H_

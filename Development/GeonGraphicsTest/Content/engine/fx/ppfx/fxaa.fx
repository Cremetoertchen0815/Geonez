/*
 *  Sample has been derived from:
 *
 *  NVIDIA FXAA 3.11 by TIMOTHY LOTTES (https://gist.github.com/bkaradzic/6011431)
 *
 */

// helper
#define FxaaTexTop(t, p) tex2D(t, p)
#define FxaaTexOff(t, p, o, r) tex2D(t, p + (o * r))
#define FxaaSat(x) saturate(x)

// flags
#define FXAA_GREEN_AS_LUMA 1

//  ("" function only)
#define FXAA_PRESET 39

/*=======FXAA  - EXTREME =====================================*/

#if (FXAA_PRESET == 39)
    #define FXAA_PS 12
    #define FXAA_P0 1.0
    #define FXAA_P1 1.0
    #define FXAA_P2 1.0
    #define FXAA_P3 1.0
    #define FXAA_P4 1.0
    #define FXAA_P5 1.5
    #define FXAA_P6 2.0
    #define FXAA_P7 2.0
    #define FXAA_P8 2.0
    #define FXAA_P9 2.0
    #define FXAA_P10 4.0
    #define FXAA_P11 8.0
#endif

/*=======APPLICATION INPUT==================================================*/
float fxaaSubpix;
float fxaaEdgeThreshold;
float fxaaEdgeThresholdMin;
// general
float invViewportWidth;
float invViewportHeight;

// texturing
sampler s0;

/*=======LUMINOSITY FUNCTION================================================*/

float FxaaLuma(float4 rgba)
{
    rgba.w = dot(rgba.rgb, float3(0.299, 0.587, 0.114));
    return rgba.w;
}

/*=======FXAA3  - PC=================================================*/

float4 FxaaPixelShader_PC(
    float2 pos,
    sampler2D tex,
    float2 fxaaRcpFrame,
    float fxaaSubpix,
    float fxaaEdgeThreshold,
    float fxaaEdgeThresholdMin)
{
    float2 posM;
    posM.x = pos.x;
    posM.y = pos.y;
    // check gather 4 alpha
    float4 rgbyM = FxaaTexTop(tex, posM);
    #if (FXAA_GREEN_AS_LUMA == 0)
        #define lumaM_PC rgbyM.w
    #else
        #define lumaM_PC rgbyM.y
    #endif
    float lumaS = FxaaLuma(FxaaTexOff(tex, posM, int2( 0, 1), fxaaRcpFrame.xy));
    float lumaE = FxaaLuma(FxaaTexOff(tex, posM, int2( 1, 0), fxaaRcpFrame.xy));
    float lumaN = FxaaLuma(FxaaTexOff(tex, posM, int2( 0,-1), fxaaRcpFrame.xy));
    float lumaW = FxaaLuma(FxaaTexOff(tex, posM, int2(-1, 0), fxaaRcpFrame.xy));

    float maxSM = max(lumaS, lumaM_PC);
    float minSM = min(lumaS, lumaM_PC);
    float maxESM = max(lumaE, maxSM);
    float minESM = min(lumaE, minSM);
    float maxWN = max(lumaN, lumaW);
    float minWN = min(lumaN, lumaW);
    float rangeMax = max(maxWN, maxESM);
    float rangeMin = min(minWN, minESM);
    float rangeMaxScaled = rangeMax * fxaaEdgeThreshold;
    float range = rangeMax - rangeMin;
    float rangeMaxClamped = max(fxaaEdgeThresholdMin, rangeMaxScaled);
    bool earlyExit = range < rangeMaxClamped;

    if (earlyExit)
        return rgbyM;

    float lumaNW = FxaaLuma(FxaaTexOff(tex, posM, int2(-1,-1), fxaaRcpFrame.xy));
    float lumaSE = FxaaLuma(FxaaTexOff(tex, posM, int2( 1, 1), fxaaRcpFrame.xy));
    float lumaNE = FxaaLuma(FxaaTexOff(tex, posM, int2( 1,-1), fxaaRcpFrame.xy));
    float lumaSW = FxaaLuma(FxaaTexOff(tex, posM, int2(-1, 1), fxaaRcpFrame.xy));

    float lumaNS = lumaN + lumaS;
    float lumaWE = lumaW + lumaE;
    float subpixRcpRange = 1.0 / range;
    float subpixNSWE = lumaNS + lumaWE;
    float edgeHorz1 = (-2.0 * lumaM_PC) + lumaNS;
    float edgeVert1 = (-2.0 * lumaM_PC) + lumaWE;

    float lumaNESE = lumaNE + lumaSE;
    float lumaNWNE = lumaNW + lumaNE;
    float edgeHorz2 = (-2.0 * lumaE) + lumaNESE;
    float edgeVert2 = (-2.0 * lumaN) + lumaNWNE;

    float lumaNWSW = lumaNW + lumaSW;
    float lumaSWSE = lumaSW + lumaSE;
    float edgeHorz4 = (abs(edgeHorz1) * 2.0) + abs(edgeHorz2);
    float edgeVert4 = (abs(edgeVert1) * 2.0) + abs(edgeVert2);
    float edgeHorz3 = (-2.0 * lumaW) + lumaNWSW;
    float edgeVert3 = (-2.0 * lumaS) + lumaSWSE;
    float edgeHorz = abs(edgeHorz3) + edgeHorz4;
    float edgeVert = abs(edgeVert3) + edgeVert4;

    float subpixNWSWNESE = lumaNWSW + lumaNESE;
    float lengthSign = fxaaRcpFrame.x;
    bool horzSpan = edgeHorz >= edgeVert;
    float subpixA = subpixNSWE * 2.0 + subpixNWSWNESE;

    if (!horzSpan)
        lumaN = lumaW;
    if (!horzSpan)
        lumaS = lumaE;
    if (horzSpan)
        lengthSign = fxaaRcpFrame.y;
    float subpixB = (subpixA * (1.0 / 12.0)) - lumaM_PC;

    float gradientN = lumaN - lumaM_PC;
    float gradientS = lumaS - lumaM_PC;
    float lumaNN = lumaN + lumaM_PC;
    float lumaSS = lumaS + lumaM_PC;
    bool pairN = abs(gradientN) >= abs(gradientS);
    float gradient = max(abs(gradientN), abs(gradientS));
    if (pairN)
        lengthSign = -lengthSign;
    float subpixC = FxaaSat(abs(subpixB) * subpixRcpRange);

    float2 posB;
    posB.x = posM.x;
    posB.y = posM.y;
    float2 offNP;
    offNP.x = (!horzSpan) ? 0.0 : fxaaRcpFrame.x;
    offNP.y = (horzSpan) ? 0.0 : fxaaRcpFrame.y;
    if (!horzSpan)
        posB.x += lengthSign * 0.5;
    if (horzSpan)
        posB.y += lengthSign * 0.5;

    float2 posN;
    posN.x = posB.x - offNP.x * FXAA_P0;
    posN.y = posB.y - offNP.y * FXAA_P0;
    float2 posP;
    posP.x = posB.x + offNP.x * FXAA_P0;
    posP.y = posB.y + offNP.y * FXAA_P0;
    float subpixD = ((-2.0) * subpixC) + 3.0;
    float lumaEndN = FxaaLuma(FxaaTexTop(tex, posN));
    float subpixE = subpixC * subpixC;
    float lumaEndP = FxaaLuma(FxaaTexTop(tex, posP));

    if (!pairN)
        lumaNN = lumaSS;
    float gradientScaled = gradient * 1.0 / 4.0;
    float lumaMM = lumaM_PC - lumaNN * 0.5;
    float subpixF = subpixD * subpixE;
    bool lumaMLTZero = lumaMM < 0.0;

    lumaEndN -= lumaNN * 0.5;
    lumaEndP -= lumaNN * 0.5;
    bool doneN = abs(lumaEndN) >= gradientScaled;
    bool doneP = abs(lumaEndP) >= gradientScaled;
    if (!doneN)
        posN.x -= offNP.x * FXAA_P1;
    if (!doneN)
        posN.y -= offNP.y * FXAA_P1;
    bool doneNP = (!doneN) || (!doneP);
    if (!doneP)
        posP.x += offNP.x * FXAA_P1;
    if (!doneP)
        posP.y += offNP.y * FXAA_P1;

    // decide on 

    if (doneNP)
    {
        if (!doneN)
            lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
        if (!doneP)
            lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
        if (!doneN)
            lumaEndN = lumaEndN - lumaNN * 0.5;
        if (!doneP)
            lumaEndP = lumaEndP - lumaNN * 0.5;
        doneN = abs(lumaEndN) >= gradientScaled;
        doneP = abs(lumaEndP) >= gradientScaled;
        if (!doneN)
            posN.x -= offNP.x * FXAA_P2;
        if (!doneN)
            posN.y -= offNP.y * FXAA_P2;
        doneNP = (!doneN) || (!doneP);
        if (!doneP)
            posP.x += offNP.x * FXAA_P2;
        if (!doneP)
            posP.y += offNP.y * FXAA_P2;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 3)
        if(doneNP) {
            if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
            if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
            if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
            if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
            doneN = abs(lumaEndN) >= gradientScaled;
            doneP = abs(lumaEndP) >= gradientScaled;
            if(!doneN) posN.x -= offNP.x * FXAA_P3;
            if(!doneN) posN.y -= offNP.y * FXAA_P3;
            doneNP = (!doneN) || (!doneP);
            if(!doneP) posP.x += offNP.x * FXAA_P3;
            if(!doneP) posP.y += offNP.y * FXAA_P3;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 4)
            if(doneNP) {
                if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                doneN = abs(lumaEndN) >= gradientScaled;
                doneP = abs(lumaEndP) >= gradientScaled;
                if(!doneN) posN.x -= offNP.x * FXAA_P4;
                if(!doneN) posN.y -= offNP.y * FXAA_P4;
                doneNP = (!doneN) || (!doneP);
                if(!doneP) posP.x += offNP.x * FXAA_P4;
                if(!doneP) posP.y += offNP.y * FXAA_P4;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 5)
                if(doneNP) {
                    if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                    if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                    if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                    if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                    doneN = abs(lumaEndN) >= gradientScaled;
                    doneP = abs(lumaEndP) >= gradientScaled;
                    if(!doneN) posN.x -= offNP.x * FXAA_P5;
                    if(!doneN) posN.y -= offNP.y * FXAA_P5;
                    doneNP = (!doneN) || (!doneP);
                    if(!doneP) posP.x += offNP.x * FXAA_P5;
                    if(!doneP) posP.y += offNP.y * FXAA_P5;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 6)
                    if(doneNP) {
                        if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                        if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                        if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                        if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                        doneN = abs(lumaEndN) >= gradientScaled;
                        doneP = abs(lumaEndP) >= gradientScaled;
                        if(!doneN) posN.x -= offNP.x * FXAA_P6;
                        if(!doneN) posN.y -= offNP.y * FXAA_P6;
                        doneNP = (!doneN) || (!doneP);
                        if(!doneP) posP.x += offNP.x * FXAA_P6;
                        if(!doneP) posP.y += offNP.y * FXAA_P6;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 7)
                        if(doneNP) {
                            if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                            if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                            if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                            if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                            doneN = abs(lumaEndN) >= gradientScaled;
                            doneP = abs(lumaEndP) >= gradientScaled;
                            if(!doneN) posN.x -= offNP.x * FXAA_P7;
                            if(!doneN) posN.y -= offNP.y * FXAA_P7;
                            doneNP = (!doneN) || (!doneP);
                            if(!doneP) posP.x += offNP.x * FXAA_P7;
                            if(!doneP) posP.y += offNP.y * FXAA_P7;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 8)
    if(doneNP) {
        if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
        if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
        if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
        if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
        doneN = abs(lumaEndN) >= gradientScaled;
        doneP = abs(lumaEndP) >= gradientScaled;
        if(!doneN) posN.x -= offNP.x * FXAA_P8;
        if(!doneN) posN.y -= offNP.y * FXAA_P8;
        doneNP = (!doneN) || (!doneP);
        if(!doneP) posP.x += offNP.x * FXAA_P8;
        if(!doneP) posP.y += offNP.y * FXAA_P8;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 9)
        if(doneNP) {
            if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
            if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
            if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
            if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
            doneN = abs(lumaEndN) >= gradientScaled;
            doneP = abs(lumaEndP) >= gradientScaled;
            if(!doneN) posN.x -= offNP.x * FXAA_P9;
            if(!doneN) posN.y -= offNP.y * FXAA_P9;
            doneNP = (!doneN) || (!doneP);
            if(!doneP) posP.x += offNP.x * FXAA_P9;
            if(!doneP) posP.y += offNP.y * FXAA_P9;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 10)
            if(doneNP) {
                if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                doneN = abs(lumaEndN) >= gradientScaled;
                doneP = abs(lumaEndP) >= gradientScaled;
                if(!doneN) posN.x -= offNP.x * FXAA_P10;
                if(!doneN) posN.y -= offNP.y * FXAA_P10;
                doneNP = (!doneN) || (!doneP);
                if(!doneP) posP.x += offNP.x * FXAA_P10;
                if(!doneP) posP.y += offNP.y * FXAA_P10;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 11)
                if(doneNP) {
                    if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                    if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                    if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                    if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                    doneN = abs(lumaEndN) >= gradientScaled;
                    doneP = abs(lumaEndP) >= gradientScaled;
                    if(!doneN) posN.x -= offNP.x * FXAA_P11;
                    if(!doneN) posN.y -= offNP.y * FXAA_P11;
                    doneNP = (!doneN) || (!doneP);
                    if(!doneP) posP.x += offNP.x * FXAA_P11;
                    if(!doneP) posP.y += offNP.y * FXAA_P11;
/*--------------------------------------------------------------------------*/
#if (FXAA_PS > 12)
                    if(doneNP) {
                        if(!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
                        if(!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
                        if(!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
                        if(!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
                        doneN = abs(lumaEndN) >= gradientScaled;
                        doneP = abs(lumaEndP) >= gradientScaled;
                        if(!doneN) posN.x -= offNP.x * FXAA_P12;
                        if(!doneN) posN.y -= offNP.y * FXAA_P12;
                        doneNP = (!doneN) || (!doneP);
                        if(!doneP) posP.x += offNP.x * FXAA_P12;
                        if(!doneP) posP.y += offNP.y * FXAA_P12;
/*--------------------------------------------------------------------------*/
                    }
#endif
/*--------------------------------------------------------------------------*/
                }
#endif
/*--------------------------------------------------------------------------*/
            }
#endif
/*--------------------------------------------------------------------------*/
        }
#endif
/*--------------------------------------------------------------------------*/
    }
#endif
/*--------------------------------------------------------------------------*/
                        }
#endif
/*--------------------------------------------------------------------------*/
                    }
#endif
/*--------------------------------------------------------------------------*/
                }
#endif
/*--------------------------------------------------------------------------*/
            }
#endif
/*--------------------------------------------------------------------------*/
        }
#endif
/*--------------------------------------------------------------------------*/
    }

    float dstN = posM.x - posN.x;
    float dstP = posP.x - posM.x;
    if (!horzSpan)
        dstN = posM.y - posN.y;
    if (!horzSpan)
        dstP = posP.y - posM.y;

    bool goodSpanN = (lumaEndN < 0.0) != lumaMLTZero;
    float spanLength = (dstP + dstN);
    bool goodSpanP = (lumaEndP < 0.0) != lumaMLTZero;
    float spanLengthRcp = 1.0 / spanLength;

    bool directionN = dstN < dstP;
    float dst = min(dstN, dstP);
    bool goodSpan = directionN ? goodSpanN : goodSpanP;
    float subpixG = subpixF * subpixF;
    float pixelOffset = (dst * (-spanLengthRcp)) + 0.5;
    float subpixH = subpixG * fxaaSubpix;

    float pixelOffsetGood = goodSpan ? pixelOffset : 0.0;
    float pixelOffsetSubpix = max(pixelOffsetGood, subpixH);
    if (!horzSpan)
        posM.x += pixelOffsetSubpix * lengthSign;
    if (horzSpan)
        posM.y += pixelOffsetSubpix * lengthSign;

    return float4(FxaaTexTop(tex, posM).rgb, lumaM_PC);
}


/*=======PIXELSHADER========================================================*/

float4 PixelShaderFunction_PC(float4 position : SV_Position, float4 color : COLOR0, float2 texCoords : TEXCOORD0) : SV_Target0
{
    float4 value = FxaaPixelShader_PC(
        texCoords,
        s0,
        float2(invViewportWidth, invViewportHeight),
        fxaaSubpix,
        fxaaEdgeThreshold,
        fxaaEdgeThresholdMin
        );

    return value;
}

/*=======TECHNIQUES=========================================================*/

technique PPFXAA
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction_PC();
    }
}
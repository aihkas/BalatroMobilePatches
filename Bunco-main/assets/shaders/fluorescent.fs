#if defined(VERTEX) || __VERSION__ > 100 || defined(GL_FRAGMENT_PRECISION_HIGH)
    #define MY_HIGHP_OR_MEDIUMP highp
#else
    #define MY_HIGHP_OR_MEDIUMP mediump
#endif

// change this variable name to your Edition's name
extern MY_HIGHP_OR_MEDIUMP vec2 fluorescent; // Using fluorescent variable

extern MY_HIGHP_OR_MEDIUMP number dissolve;
extern MY_HIGHP_OR_MEDIUMP number time;
extern MY_HIGHP_OR_MEDIUMP vec4 texture_details;
extern MY_HIGHP_OR_MEDIUMP vec2 image_details;
extern bool shadow;
extern MY_HIGHP_OR_MEDIUMP vec4 burn_colour_1;
extern MY_HIGHP_OR_MEDIUMP vec4 burn_colour_2;

// dissolve_mask function (using float literals consistently)
vec4 dissolve_mask(vec4 tex, vec2 texture_coords, vec2 uv)
{
    if (dissolve < 0.001) {
        return vec4(shadow ? vec3(0.0, 0.0, 0.0) : tex.xyz, shadow ? tex.a*0.3: tex.a);
    }
    float adjusted_dissolve = (dissolve*dissolve*(3.0-2.0*dissolve))*1.02 - 0.01;
    float t = time * 10.0 + 2003.0;
    float max_tex_detail = max(0.0001, max(texture_details.b, texture_details.a));
    vec2 floored_uv = (floor((uv*texture_details.ba))) / max_tex_detail;
    vec2 uv_scaled_centered = (floored_uv - 0.5) * 2.3 * max_tex_detail;
    vec2 field_part1 = uv_scaled_centered + 50.0*vec2(sin(-t / 143.6340), cos(-t / 99.4324));
    vec2 field_part2 = uv_scaled_centered + 50.0*vec2(cos( t / 53.1532),  cos( t / 61.4532));
    vec2 field_part3 = uv_scaled_centered + 50.0*vec2(sin(-t / 87.53218), sin(-t / 49.0000));
    float field = (1.0 + (
        cos(length(field_part1) / 19.483) + sin(length(field_part2) / 33.155) * cos(field_part2.y / 15.73) +
        cos(length(field_part3) / 27.193) * sin(field_part3.x / 21.92) ))/2.0;
    vec2 borders = vec2(0.2, 0.8);
    float res = (0.5 + 0.5* cos( (adjusted_dissolve) / 82.612 + ( field + -0.5 ) *3.14))
    - (floored_uv.x > borders.y ? (floored_uv.x - borders.y)*(5.0 + 5.0*dissolve) : 0.0)*(dissolve)
    - (floored_uv.y > borders.y ? (floored_uv.y - borders.y)*(5.0 + 5.0*dissolve) : 0.0)*(dissolve)
    - (floored_uv.x < borders.x ? (borders.x - floored_uv.x)*(5.0 + 5.0*dissolve) : 0.0)*(dissolve)
    - (floored_uv.y < borders.x ? (borders.x - floored_uv.y)*(5.0 + 5.0*dissolve) : 0.0)*(dissolve);
    if (tex.a > 0.01 && burn_colour_1.a > 0.01 && !shadow && res < adjusted_dissolve + 0.8*(0.5-abs(adjusted_dissolve-0.5)) && res > adjusted_dissolve) {
        if (!shadow && res < adjusted_dissolve + 0.5*(0.5-abs(adjusted_dissolve-0.5)) && res > adjusted_dissolve) {
            tex.rgba = burn_colour_1.rgba;
        } else if (burn_colour_2.a > 0.01) {
            tex.rgba = burn_colour_2.rgba;
        }
    }
    return vec4(shadow ? vec3(0.0, 0.0, 0.0) : tex.xyz, res > adjusted_dissolve ? (shadow ? tex.a*0.3: tex.a) : 0.0);
}

// Helper functions (distanceBetweenColors, rgb2hsv, hsv2rgb, hueshift, saturation) - No changes needed here
float distanceBetweenColors(vec3 colorA, vec3 colorB) {
    return length(clamp(colorA, 0.0, 1.0) - clamp(colorB, 0.0, 1.0));
}
vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
vec3 hueshift(vec3 rgb, float adjustment) {
    vec3 hsv = rgb2hsv(rgb);
    hsv.x += adjustment;
    return hsv2rgb(hsv);
}
vec3 saturation(vec3 rgb, float adjustment) {
    const vec3 W = vec3(0.1125, 0.2154, 0.0221);
    vec3 intensity = vec3(dot(rgb, W));
    return mix(intensity, rgb, adjustment);
}


// effect function
vec4 effect( vec4 colour, Image texture, vec2 texture_coords, vec2 screen_coords )
{
    vec4 tex = Texel(texture, texture_coords);
    vec3 pixelColor = tex.rgb;

    vec2 uv = vec2(0.0);
    if (texture_details.b != 0.0 && texture_details.a != 0.0) {
         uv = (((texture_coords)*(image_details)) - texture_details.xy*texture_details.ba)/texture_details.ba;
    }

    if (uv.x > 9999.0){
        uv = fluorescent * 0.0;
    }

    // *** THE FIX IS HERE ***
    // Declare arrays first
    vec3 targetColors[6];
    float sharpnessMultipliers[6];

    // Initialize arrays element by element
    targetColors[0] = vec3(1.0, 0.0, 0.0); // Red
    targetColors[1] = vec3(0.0, 1.0, 0.0); // Green
    targetColors[2] = vec3(0.0, 0.0, 1.0); // Blue
    targetColors[3] = vec3(1.0, 1.0, 0.0); // Yellow
    targetColors[4] = vec3(0.0, 1.0, 1.0); // Cyan
    targetColors[5] = vec3(1.0, 0.0, 1.0); // Magenta

    sharpnessMultipliers[0] = 3.0; // Red
    sharpnessMultipliers[1] = 3.0; // Green
    sharpnessMultipliers[2] = 3.0; // Blue
    sharpnessMultipliers[3] = 3.0; // Yellow
    sharpnessMultipliers[4] = 3.0; // Cyan
    sharpnessMultipliers[5] = 3.0; // Magenta
    // *** END OF FIX ***

    float effect_strength = 0.7 + (0.3 * clamp(cos(sin(uv.y * 8.24 + fluorescent.x * 6.0) + sin(uv.x * 6.12 + fluorescent.x * 2.0) * fluorescent.y * uv.y * 3.15), 0.0, 1.0));

    float totalWeight = 0.0;
    vec3 blendedColor = vec3(0.0);

    // Loop uses the initialized arrays
    for (int i = 0; i < 6; i++) {
        float distance = distanceBetweenColors(pixelColor, targetColors[i]);
        float weight = exp(-distance * sharpnessMultipliers[i]);
        totalWeight += weight;
        vec3 saturatedColor = saturation(pixelColor, 3.0);
        blendedColor += saturatedColor * weight;
    }

    if (totalWeight > 0.0) {
        blendedColor /= totalWeight;
    }

    float desaturationFactor = smoothstep(0.0, 1.0, totalWeight);

    blendedColor = rgb2hsv(blendedColor);
    blendedColor.z *= desaturationFactor * blendedColor.y;
    blendedColor.y = 1.0;
    blendedColor = hsv2rgb(blendedColor);

    vec3 tintedColor = pixelColor * vec3(0.28, 0.35, 0.49);
    tintedColor -= (tintedColor * (effect_strength - 1.0)) * 0.15;

    vec3 finalColor = blendedColor * effect_strength + tintedColor;

    return dissolve_mask(vec4(finalColor.rgb, tex.a * tex.a), texture_coords, uv);
}

// for transforming the card while your mouse is on it
extern MY_HIGHP_OR_MEDIUMP vec2 mouse_screen_pos;
extern MY_HIGHP_OR_MEDIUMP float hovering;
extern MY_HIGHP_OR_MEDIUMP float screen_scale;
// love_ScreenSize is provided automatically

#ifdef VERTEX
// The 'love_ScreenSize' uniform is automatically available here
vec4 position( mat4 transform_projection, vec4 vertex_position )
{
    if (hovering <= 0.0){
        return transform_projection * vertex_position;
    }
    float screenSizeLen = length(love_ScreenSize.xy);
    if (screenSizeLen < 0.0001) {
         return transform_projection * vertex_position;
    }
    float mid_dist = length(vertex_position.xy - 0.5 * love_ScreenSize.xy) / screenSizeLen;
    vec2 mouse_offset = (vertex_position.xy - mouse_screen_pos.xy) / screen_scale;
    float scale = 0.2 * (-0.03 - 0.3 * max(0.0, 0.3 - mid_dist))
                * hovering * (length(mouse_offset) * length(mouse_offset)) / max(0.001, 2.0 - mid_dist);
    return transform_projection * vertex_position + vec4(0.0, 0.0, 0.0, scale);
}
#endif
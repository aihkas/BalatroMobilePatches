#if defined(VERTEX) || __VERSION__ > 100 || defined(GL_FRAGMENT_PRECISION_HIGH)
    #define MY_HIGHP_OR_MEDIUMP highp
#else
    #define MY_HIGHP_OR_MEDIUMP mediump
#endif

// change this variable name to your Edition's name
// YOU MUST USE THIS VARIABLE IN THE vec4 effect AT LEAST ONCE
// ^^ CRITICALLY IMPORTANT (IDK WHY)
extern MY_HIGHP_OR_MEDIUMP vec3 headache;

extern MY_HIGHP_OR_MEDIUMP number dissolve;
extern MY_HIGHP_OR_MEDIUMP number time;
extern MY_HIGHP_OR_MEDIUMP vec4 texture_details;
extern MY_HIGHP_OR_MEDIUMP vec2 image_details;
extern bool shadow;
extern MY_HIGHP_OR_MEDIUMP vec4 burn_colour_1;
extern MY_HIGHP_OR_MEDIUMP vec4 burn_colour_2;

// Required functions (dissolve_mask, layerTexel, alphaBlend)
vec4 dissolve_mask(vec4 tex, vec2 texture_coords, vec2 uv)
{
    if (dissolve < 0.001) {
        return vec4(shadow ? vec3(0.0, 0.0, 0.0) : tex.xyz, shadow ? tex.a * 0.3 : tex.a); // Use 0.0 literals
    }

    float adjusted_dissolve = (dissolve*dissolve*(3.0-2.0*dissolve))*1.02 - 0.01; // Use 0.0 literals

    float t = time * 10.0 + 2003.0; // Use 0.0 literals
    vec2 floored_uv = (floor((uv*texture_details.ba)))/max(texture_details.b, texture_details.a);
    vec2 uv_scaled_centered = (floored_uv - 0.5) * 2.3 * max(texture_details.b, texture_details.a);

    vec2 field_part1 = uv_scaled_centered + 50.0*vec2(sin(-t / 143.6340), cos(-t / 99.4324)); // Use 0.0 literals
    vec2 field_part2 = uv_scaled_centered + 50.0*vec2(cos( t / 53.1532),  cos( t / 61.4532)); // Use 0.0 literals
    vec2 field_part3 = uv_scaled_centered + 50.0*vec2(sin(-t / 87.53218), sin(-t / 49.0000)); // Use 0.0 literals

    float field = (1.0 + ( // Use 0.0 literals
        cos(length(field_part1) / 19.483) + sin(length(field_part2) / 33.155) * cos(field_part2.y / 15.73) +
        cos(length(field_part3) / 27.193) * sin(field_part3.x / 21.92) )) / 2.0; // Use 0.0 literals
    vec2 borders = vec2(0.2, 0.8);

    // Use 0.0 literals consistently
    float res = (0.5 + 0.5 * cos( (adjusted_dissolve) / 82.612 + ( field + -0.5 ) * 3.14))
    - (floored_uv.x > borders.y ? (floored_uv.x - borders.y) * (5.0 + 5.0 * dissolve) : 0.0) * (dissolve)
    - (floored_uv.y > borders.y ? (floored_uv.y - borders.y) * (5.0 + 5.0 * dissolve) : 0.0) * (dissolve)
    - (floored_uv.x < borders.x ? (borders.x - floored_uv.x) * (5.0 + 5.0 * dissolve) : 0.0) * (dissolve)
    - (floored_uv.y < borders.x ? (borders.x - floored_uv.y) * (5.0 + 5.0 * dissolve) : 0.0) * (dissolve);

    if (tex.a > 0.01 && burn_colour_1.a > 0.01 && !shadow && res < adjusted_dissolve + 0.8*(0.5-abs(adjusted_dissolve-0.5)) && res > adjusted_dissolve) {
        if (!shadow && res < adjusted_dissolve + 0.5*(0.5-abs(adjusted_dissolve-0.5)) && res > adjusted_dissolve) {
            tex.rgba = burn_colour_1.rgba;
        } else if (burn_colour_2.a > 0.01) {
            tex.rgba = burn_colour_2.rgba;
        }
    }

    return vec4(shadow ? vec3(0.0, 0.0, 0.0) : tex.xyz, res > adjusted_dissolve ? (shadow ? tex.a*0.3: tex.a) : 0.0); // Use 0.0 literals
}

vec4 layerTexel( Image texture, vec2 uv, int frame, vec2 displace, float parallax, bool tile ) {
    vec2 duv = uv + (displace * parallax * 0.0001);
    vec4 col = vec4(0.0);
    if (tile) {
        // Ensure image_details components are not zero before division
        if (image_details.x != 0.0 && image_details.y != 0.0) {
             duv = mod(duv + vec2(headache.z * 10.0) / image_details, vec2(69.0 / image_details.x, 69.0 / image_details.y));
             vec2 fuv = duv + (vec2((float(frame) * 71.0) + 1.0, 1.0) / image_details);
             col = Texel(texture, fuv);
        }
    }
    else {
         // Ensure image_details.x is not zero before division
        if (image_details.x != 0.0 && (duv.x >= 0.0) && (duv.x <= 71.0 / image_details.x) && (duv.y >= 0.0) && (duv.y <= 1.0)) {
            vec2 fuv = duv + (vec2(float(frame) * 71.0, 0.0) / image_details);
            col = Texel(texture, fuv);
        }
    }
    return col;
}

vec4 alphaBlend( vec4 source, vec4 destination ) {
    // Avoid division by zero if outAlpha is zero
    float outAlpha = source.a + destination.a * (1.0 - source.a);
    if (outAlpha < 0.0001) { // Use a small epsilon
        return vec4(0.0, 0.0, 0.0, 0.0); // Or return destination, depending on desired behavior
    }
    vec3 outColor = (source.rgb * source.a + destination.rgb * destination.a * (1.0 - source.a)) / outAlpha;
    return vec4(outColor, outAlpha);
}


vec4 effect( vec4 colour, Image texture, vec2 texture_coords, vec2 screen_coords )
{
    vec4 tex = Texel(texture, texture_coords);
    // Ensure maskTex is initialized even if layerTexel returns zero alpha
    vec4 maskTex = layerTexel(texture, texture_coords, 1, headache.xy, 0.0, false);
    // No need to check maskTex.a here as layerTexel returns vec4(0.0) if out of bounds or div by zero

    vec4 blendedTex = tex;

    vec3 pink = vec3(1.0, 0.0, 0.45);

    float mult = 5.0;
    float steps = 0.25; // 5 steps: 0.0, 0.25, 0.5, 0.75, 1.0

    // Use float literals for loop bounds and initialization
    for (float i = 0.0; i <= 1.0; i += steps) {
        vec4 layerTex = layerTexel(texture, texture_coords, 4, headache.xy, 14.0 + mult - i * mult, true);
        // Only blend if mask allows and layer has alpha
        if (maskTex.a > 0.01 && layerTex.a > 0.01) {
           layerTex.a *= maskTex.a;
           layerTex.a *= i * 0.3; // Scale alpha
           layerTex.rgb = mix(layerTex.rgb, pink, i * i); // Mix color
           blendedTex = alphaBlend(layerTex, blendedTex);
        }
    }

    for (float i = 0.0; i <= 1.0; i += steps) {
        vec4 layerTex = layerTexel(texture, texture_coords, 3, headache.xy, 5.0 + mult - i * mult, false);
        if (maskTex.a > 0.01 && layerTex.a > 0.01) {
           layerTex.a *= maskTex.a;
           layerTex.a *= i; // Scale alpha
           layerTex.rgb = mix(layerTex.rgb, pink, i * i); // Mix color
           blendedTex = alphaBlend(layerTex, blendedTex);
        }
    }

    for (float i = 0.0; i <= 1.0; i += steps) {
        vec4 layerTex = layerTexel(texture, texture_coords, 2, headache.xy, 2.5 + mult - i * mult, false);
        if (maskTex.a > 0.01 && layerTex.a > 0.01) {
           layerTex.a *= maskTex.a;
           layerTex.a *= i; // Scale alpha
           layerTex.rgb = mix(layerTex.rgb, pink, i * i); // Mix color
           blendedTex = alphaBlend(layerTex, blendedTex);
        }
    }

    // Ensure texture_details.ba components are not zero before division
    vec2 uv = vec2(0.0); // Initialize uv
    if (texture_details.b != 0.0 && texture_details.a != 0.0) {
         uv = (((texture_coords) * (image_details)) - texture_details.xy * texture_details.ba) / texture_details.ba;
    }

    // Dummy use of 'headache' - this comparison is always false unless uv.x is negative
    // A slightly more robust dummy usage might involve adding a tiny value:
    if (uv.x < -9999.0){ // Condition likely always false
        blendedTex.r += headache.x * 0.0; // Minimal, likely optimized-out usage
    }
    return dissolve_mask(blendedTex, texture_coords, uv);
}

// for transforming the card while your mouse is on it
extern MY_HIGHP_OR_MEDIUMP vec2 mouse_screen_pos;
extern MY_HIGHP_OR_MEDIUMP float hovering;
extern MY_HIGHP_OR_MEDIUMP float screen_scale;
// love_ScreenSize is provided by LÖVE/Balatro automatically
// *** REMOVED THE extern DECLARATION FOR love_ScreenSize ***


#ifdef VERTEX
// The 'love_ScreenSize' uniform is automatically available here
vec4 position( mat4 transform_projection, vec4 vertex_position )
{
    // Use float literal for comparison
    if (hovering <= 0.0){
        return transform_projection * vertex_position;
    }

    // Avoid division by zero if length is zero
    float screenSizeLen = length(love_ScreenSize.xy);
    if (screenSizeLen < 0.0001) {
         // Handle case where screen size is zero (unlikely but safe)
         return transform_projection * vertex_position;
    }

    // Ensure float literals in calculations
    float mid_dist = length(vertex_position.xy - 0.5 * love_ScreenSize.xy) / screenSizeLen;
    vec2 mouse_offset = (vertex_position.xy - mouse_screen_pos.xy) / screen_scale;
    // Ensure float literals in calculations (0.0 instead of 0., 2.0 instead of 2.)
    // Avoid division by zero/small numbers in the denominator
    float scale = 0.2 * (-0.03 - 0.3 * max(0.0, 0.3 - mid_dist))
                * hovering * (length(mouse_offset) * length(mouse_offset)) / max(0.001, 2.0 - mid_dist);

    // Use float literals (0.0 or 0.) in the vec4 constructor
    return transform_projection * vertex_position + vec4(0.0, 0.0, 0.0, scale);
}
#endif
#if defined(VERTEX) || __VERSION__ > 100 || defined(GL_FRAGMENT_PRECISION_HIGH)
    #define MY_HIGHP_OR_MEDIUMP highp
#else
    #define MY_HIGHP_OR_MEDIUMP mediump
#endif

// change this variable name to your Edition's name
extern MY_HIGHP_OR_MEDIUMP vec2 glitter; // Using glitter variable

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
        // Use 0.0 literals
        return vec4(shadow ? vec3(0.0, 0.0, 0.0) : tex.xyz, shadow ? tex.a*0.3: tex.a);
    }

    // Use 0.0 literals
    float adjusted_dissolve = (dissolve*dissolve*(3.0-2.0*dissolve))*1.02 - 0.01;

    float t = time * 10.0 + 2003.0; // Use 0.0 literal
    // Avoid division by zero
    float max_tex_detail = max(0.0001, max(texture_details.b, texture_details.a));
    vec2 floored_uv = (floor((uv*texture_details.ba))) / max_tex_detail;
    vec2 uv_scaled_centered = (floored_uv - 0.5) * 2.3 * max_tex_detail; // Use max_tex_detail here too

    // Use 0.0 literals
    vec2 field_part1 = uv_scaled_centered + 50.0*vec2(sin(-t / 143.6340), cos(-t / 99.4324));
    vec2 field_part2 = uv_scaled_centered + 50.0*vec2(cos( t / 53.1532),  cos( t / 61.4532));
    vec2 field_part3 = uv_scaled_centered + 50.0*vec2(sin(-t / 87.53218), sin(-t / 49.0000));

    // Use 0.0 literals
    float field = (1.0 + (
        cos(length(field_part1) / 19.483) + sin(length(field_part2) / 33.155) * cos(field_part2.y / 15.73) +
        cos(length(field_part3) / 27.193) * sin(field_part3.x / 21.92) ))/2.0;
    vec2 borders = vec2(0.2, 0.8);

    // Use 0.0 literals
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
    // Use 0.0 literals
    return vec4(shadow ? vec3(0.0, 0.0, 0.0) : tex.xyz, res > adjusted_dissolve ? (shadow ? tex.a*0.3: tex.a) : 0.0);
}


// hue function (using float literals consistently)
number hue(number s, number t, number h)
{
    number hs = mod(h, 1.0)*6.0; // Use 0.0 literals
    if (hs < 1.0) return (t-s) * hs + s; // Use 0.0 literal
    if (hs < 3.0) return t; // Use 0.0 literal
    if (hs < 4.0) return (t-s) * (4.0-hs) + s; // Use 0.0 literal
    return s;
}

// RGB function (using float literals consistently)
vec4 RGB(vec4 c)
{
    if (c.y < 0.0001)
        return vec4(vec3(c.z), c.a);

    // Use 0.0 literal
    number t = (c.z < 0.5) ? c.y*c.z + c.z : -c.y*c.z + (c.y+c.z);
    number s = 2.0 * c.z - t; // Use 0.0 literal
    // Use 0.0 literals
    return vec4(hue(s,t,c.x + 1.0/3.0), hue(s,t,c.x), hue(s,t,c.x - 1.0/3.0), c.w);
}

// HSL function (using float literals consistently)
vec4 HSL(vec4 c)
{
    number low = min(c.r, min(c.g, c.b));
    number high = max(c.r, max(c.g, c.b));
    number delta = high - low;
    number sum = high+low;

    // Use 0.0 literals
    vec4 hsl = vec4(0.0, 0.0, 0.5 * sum, c.a);
    if (delta == 0.0) // Use 0.0 literal
        return hsl;

    // Use 0.0 literals
    hsl.y = (hsl.z < 0.5) ? delta / sum : delta / (2.0 - sum);

    // Use 0.0 literals
    if (high == c.r)
        hsl.x = (c.g - c.b) / delta;
    else if (high == c.g)
        hsl.x = (c.b - c.r) / delta + 2.0;
    else
        hsl.x = (c.r - c.g) / delta + 4.0;

    // Use 0.0 literals
    hsl.x = mod(hsl.x / 6.0, 1.0);
    return hsl;
}

// GLSL Simplex noise function (already using float literals)
vec3 mod289(vec3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
vec4 mod289(vec4 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
vec4 permute(vec4 x) {
  return mod289(((x*34.0)+1.0)*x);
}
vec4 taylorInvSqrt(vec4 r) {
  return 1.79284291400159 - 0.85373472095314 * r;
}
vec3 fade(vec3 t) {
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}
float cnoise(vec3 P) {
  vec3 Pi0 = floor(P);
  vec3 Pi1 = Pi0 + vec3(1.0);
  Pi0 = mod289(Pi0);
  Pi1 = mod289(Pi1);
  vec3 Pf0 = fract(P);
  vec3 Pf1 = Pf0 - vec3(1.0);
  vec4 ix = vec4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
  vec4 iy = vec4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
  vec4 iz0 = vec4(Pi0.z);
  vec4 iz1 = vec4(Pi1.z);
  vec4 ixy = permute(permute(ix) + iy);
  vec4 ixy0 = permute(ixy + iz0);
  vec4 ixy1 = permute(ixy + iz1);
  vec4 gx0 = ixy0 * (1.0 / 7.0);
  vec4 gy0 = fract(floor(gx0) * (1.0 / 7.0)) - 0.5;
  gx0 = fract(gx0);
  vec4 gz0 = vec4(0.5) - abs(gx0) - abs(gy0);
  vec4 sz0 = step(gz0, vec4(0.0));
  gx0 -= sz0 * (step(0.0, gx0) - 0.5);
  gy0 -= sz0 * (step(0.0, gy0) - 0.5);
  vec4 gx1 = ixy1 * (1.0 / 7.0);
  vec4 gy1 = fract(floor(gx1) * (1.0 / 7.0)) - 0.5;
  gx1 = fract(gx1);
  vec4 gz1 = vec4(0.5) - abs(gx1) - abs(gy1);
  vec4 sz1 = step(gz1, vec4(0.0));
  gx1 -= sz1 * (step(0.0, gx1) - 0.5);
  gy1 -= sz1 * (step(0.0, gy1) - 0.5);
  vec3 g000 = vec3(gx0.x,gy0.x,gz0.x);
  vec3 g100 = vec3(gx0.y,gy0.y,gz0.y);
  vec3 g010 = vec3(gx0.z,gy0.z,gz0.z);
  vec3 g110 = vec3(gx0.w,gy0.w,gz0.w);
  vec3 g001 = vec3(gx1.x,gy1.x,gz1.x);
  vec3 g101 = vec3(gx1.y,gy1.y,gz1.y);
  vec3 g011 = vec3(gx1.z,gy1.z,gz1.z);
  vec3 g111 = vec3(gx1.w,gy1.w,gz1.w);
  vec4 norm0 = taylorInvSqrt(vec4(dot(g000,g000), dot(g010,g010), dot(g100,g100), dot(g110,g110)));
  g000 *= norm0.x;
  g010 *= norm0.y;
  g100 *= norm0.z;
  g110 *= norm0.w;
  vec4 norm1 = taylorInvSqrt(vec4(dot(g001,g001), dot(g011,g011), dot(g101,g101), dot(g111,g111)));
  g001 *= norm1.x;
  g011 *= norm1.y;
  g101 *= norm1.z;
  g111 *= norm1.w;
  float n000 = dot(g000, Pf0);
  float n100 = dot(g100, vec3(Pf1.x, Pf0.yz));
  float n010 = dot(g010, vec3(Pf0.x, Pf1.y, Pf0.z));
  float n110 = dot(g110, vec3(Pf1.xy, Pf0.z));
  float n001 = dot(g001, vec3(Pf0.xy, Pf1.z));
  float n101 = dot(g101, vec3(Pf1.x, Pf0.y, Pf1.z));
  float n011 = dot(g011, vec3(Pf0.x, Pf1.yz));
  float n111 = dot(g111, Pf1);
  vec3 fade_xyz = fade(Pf0);
  vec4 n_z = mix(vec4(n000, n100, n010, n110), vec4(n001, n101, n011, n111), fade_xyz.z);
  vec2 n_yz = mix(n_z.xy, n_z.zw, fade_xyz.y);
  float n_xyz = mix(n_yz.x, n_yz.y, fade_xyz.x);
  return 2.2 * n_xyz;
}

// Lighten blending mode
vec4 lighten(vec4 colour1, vec4 colour2) {
    vec4 result;
    result.r = max(colour1.r, colour2.r);
    result.g = max(colour1.g, colour2.g);
    result.b = max(colour1.b, colour2.b);
    result.a = max(colour1.a, colour2.a); // Keep original alpha logic for blend modes usually
    return result;
}

// effect function (using float literals consistently)
vec4 effect( vec4 colour, Image texture, vec2 texture_coords, vec2 screen_coords )
{
    vec4 tex = Texel(texture, texture_coords);

    // Avoid division by zero
    vec2 uv = vec2(0.0); // Initialize
    if (texture_details.b != 0.0 && texture_details.a != 0.0) {
         uv = (((texture_coords)*(image_details)) - texture_details.xy*texture_details.ba)/texture_details.ba;
    }

    // Dummy use of 'glitter' variable
    if (uv.x > 9999.0){ // Condition likely always false
        uv = glitter * 0.0; // Minimal, likely optimized-out usage
    }

    float mod = glitter.r * 2.0; // Using .r component of glitter vec2
    float glitter_amount = 0.15;
    float saturation_amount = 5.0;
    float glitter_brightness = 3.0;

    // Use 0.0 literals
    vec4 colour_1 = vec4(0.188, 0.471, 0.875, 1.0); // Blue
    vec4 colour_2 = vec4(0.875, 0.188, 0.222, 0.8); // Crimson
    vec4 colour_3 = vec4(0.416, 0.573, 0.369, 0.6); // Greenish

    // Use 0.0 literals
    float noise = cnoise(vec3(uv * 50.0, 3.0 * mod));
    float antinoise = cnoise(vec3(uv * 30.0, 2.0 * mod));

    // Use 0.0 literals
    vec4 grad = mix(colour_1, colour_2, uv.x + uv.y + sin(mod) - 1.0);
    grad =      mix(grad,    colour_3, uv.y - uv.x + cos(mod) + 1.0);

    // Use 0.0 literal
    float spark = max(0.0, noise - antinoise - glitter_amount);

    // Use 0.0 literals
    vec4 saturated_colour = HSL(mix(tex, grad, 0.2));

    saturated_colour.g *= saturation_amount;
    saturated_colour.b = 0.3; // Lightness adjustment

    saturated_colour = RGB(saturated_colour); // Back to RGB

    saturated_colour.r *= (saturated_colour.r * 0.9); // Adjust red component

    // Ensure glitter_brightness is not zero
    saturated_colour = lighten(tex, saturated_colour * 5.0) / max(0.001, glitter_brightness);

    // Use 0.0 literals
    vec4 final_colour = lighten(mix((tex - 0.4), saturated_colour * spark, 1.0), grad * spark * 0.5 ); // Experiment with blending spark better

    // Use 0.0 literal for the final mix factor if needed, otherwise keep original logic if it worked
    // Original logic: colour = lighten(mix((colour - 0.4) + (saturated_colour * spark), grad, 0.03), grad);
    // Let's try blending the original colour with the spark effect, then lighten with the gradient slightly based on spark
    vec4 spark_effect = saturated_colour * spark;
    // Blend original texture (maybe adjusted) with spark effect
    vec4 base_plus_spark = mix(tex * 0.8, spark_effect, spark); // Mix based on spark amount, dim original slightly

    // Lighten with gradient based on spark intensity as well
    final_colour = lighten(base_plus_spark, grad * spark * 0.3);


    // Required: pass the calculated color to dissolve_mask
    return dissolve_mask(final_colour, texture_coords, uv);
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
    // Use float literal for comparison
    if (hovering <= 0.0){
        return transform_projection * vertex_position;
    }

    // Avoid division by zero if length is zero
    float screenSizeLen = length(love_ScreenSize.xy);
    if (screenSizeLen < 0.0001) {
         return transform_projection * vertex_position;
    }

    // Ensure float literals in calculations
    float mid_dist = length(vertex_position.xy - 0.5 * love_ScreenSize.xy) / screenSizeLen;
    vec2 mouse_offset = (vertex_position.xy - mouse_screen_pos.xy) / screen_scale;

    // *** THE FIX IS HERE (and related lines) ***
    // Ensure float literals (0.0 instead of 0., 2.0 instead of 2.)
    // Avoid division by zero/small numbers in the denominator
    float scale = 0.2 * (-0.03 - 0.3 * max(0.0, 0.3 - mid_dist)) // Use 0.0
                * hovering * (length(mouse_offset) * length(mouse_offset)) / max(0.001, 2.0 - mid_dist); // Use 2.0 and max guard

    // Use float literals (0.0 or 0.) in the vec4 constructor
    return transform_projection * vertex_position + vec4(0.0, 0.0, 0.0, scale);
}
#endif
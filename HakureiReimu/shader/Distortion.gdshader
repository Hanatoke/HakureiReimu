shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

uniform float amplitude : hint_range(0.01, 1.0)= 0.03;
uniform float frequency : hint_range(1, 20.0)= 10.0;
uniform float speed : hint_range(0, 10.0)= 5.0;
uniform vec2 direction = vec2(1.0, 0.0);

void fragment() {
    vec2 screen_uv = SCREEN_UV;

    float mask = texture(TEXTURE, UV).a;

    vec2 p = UV - vec2(0.5);
    float dist = length(p);

    float wave = sin(dist * frequency - TIME * speed);

    vec2 radial = dist > 0.0 ? normalize(p) : vec2(0.0);

    vec2 dir = normalize(radial + direction * 0.5);

    vec2 offset = dir * wave * amplitude * mask;

    COLOR = texture(screen_texture, screen_uv + offset);
}
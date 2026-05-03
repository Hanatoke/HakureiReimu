shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

uniform float amplitude : hint_range(0.01, 1.0)= 0.03;
uniform float frequency : hint_range(1, 20.0)= 10.0;
uniform float speed : hint_range(0, 10.0)= 5.0;
uniform float width : hint_range(0, 2)= 0.3;
uniform vec2 direction = vec2(1.0, 0.0);

void fragment() {
    vec2 uv = SCREEN_UV;

    float mask = texture(TEXTURE, UV).a;

    vec2 p = UV - vec2(0.5);

    vec2 dir = normalize(direction);
	
    vec2 right = vec2(-dir.y, dir.x);

    float forward = dot(p, dir);

    float side = dot(p, right);

    float falloff = exp(-abs(side) / width);

    float wave = sin(forward * frequency - TIME * speed);

    vec2 offset = dir * wave * amplitude * falloff * mask;

    COLOR = texture(screen_texture, uv + offset);
}
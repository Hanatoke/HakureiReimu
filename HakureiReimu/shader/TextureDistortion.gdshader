shader_type canvas_item;

uniform sampler2D screen_tex;
uniform vec2 twist = vec2(0.0, 0.0);

void fragment() {
    vec2 uv = UV;

    vec2 centered = uv - vec2(0.5);

    centered.x += twist.x * pow(centered.y, 2.0);
    centered.y += twist.y * pow(centered.x, 2.0);

    vec2 uv_new = centered + vec2(0.5);
    uv_new = clamp(uv_new, vec2(0.0), vec2(1.0));

    COLOR = texture(screen_tex, uv_new);
}
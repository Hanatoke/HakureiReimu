shader_type canvas_item;

uniform vec2 twist = vec2(0.0, 0.0); 

void fragment() {
    vec2 centered = UV - vec2(0.5);

    centered.x += twist.x * pow(centered.y, 2.0); // Y 控制 X 偏移
    centered.y += twist.y * pow(centered.x, 2.0); // X 控制 Y 偏移

    vec2 uv_new = centered + vec2(0.5);

    uv_new = clamp(uv_new, vec2(0.0), vec2(1.0));

    COLOR = texture(TEXTURE, uv_new) * COLOR;
}
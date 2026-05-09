shader_type canvas_item;
render_mode blend_add;
uniform float speed : hint_range(0, 10) = 0.5;   // 彩虹滚动速度
uniform float width_fade : hint_range(0, 1) = 1.0; // 是否沿宽度淡出
uniform vec2 gradient_dir  = vec2(1,0);

vec3 rainbow(float t) {
    return vec3(
        0.5 + 0.5 * sin(6.2831 * (t + 0.0)),
        0.5 + 0.5 * sin(6.2831 * (t + 0.333)),
        0.5 + 0.5 * sin(6.2831 * (t + 0.666))
    );
}

void fragment() {
    vec2 dir = normalize(gradient_dir);

    float t = dot(UV, dir) + TIME * speed;

    vec3 col = rainbow(t);

    float fade = 1.0;
    if (width_fade > 0.0) {
        fade = 1.0 - abs(UV.y - 0.5) * 2.0; 
    }

    float alpha = COLOR.a * fade;

    COLOR = vec4(col, alpha);
}
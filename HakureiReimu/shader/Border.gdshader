shader_type canvas_item;

// 彩虹色旋转边框着色器
uniform float speed : hint_range(-5, 5.0) = 0.5;

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
void fragment() {
    // 获取UV坐标并偏移中心至(0.5, 0.5)
    vec2 uv = UV - vec2(0.5);
    
    // 计算角度和半径
    float angle = atan(uv.y, uv.x);
    float radius = length(uv);
   
    // 动态颜色计算（基于时间与角度）
    float hue = fract(angle / (2.0 * 3.14159) + TIME * speed);
    vec3 rainbowColor = hsv2rgb(vec3(hue, 1.0, 1.0));
    float alpha=COLOR.a;
	COLOR = vec4(rainbowColor, texture(TEXTURE, UV).a*alpha);
}


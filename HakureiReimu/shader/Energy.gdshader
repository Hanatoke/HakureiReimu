shader_type canvas_item;
// 时间控制参数，单位为秒
uniform float speed : hint_range(0.1, 5.0) = 1.25;
// 彩虹颜色密度调节
uniform float frequency : hint_range(0.1, 10.0) = 3.5;
uniform float brightness : hint_range(0.1, 10.0) = 2;
uniform float modify : hint_range(0.1, 1) = 0.8;
uniform float offset : hint_range(0, 10) = 0;
// HSV to RGB conversion helper function
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
void fragment() {
	// 获取当前纹理坐标的UV值并转换为中心对称坐标 (-0.5 ~ 0.5)
	vec2 center_uv = UV - vec2(0.5);
	// 计算距离中心的距离（极坐标半径）
	float distance_from_center = length(center_uv);
	
	// 使用时间变量来驱动波纹向外扩展的效果
	float time_factor = TIME * speed+offset;
	
	// 利用sin函数制造一圈圈扩散出去的动态效果
	float wave_value = sin(distance_from_center * frequency - time_factor) * 0.5 + modify;
	
	// 构造HSV颜色空间下的彩虹色相(Hue)，饱和度(Saturation)=1，亮度(Value)=wave_value
	vec3 hsv_color = vec3(wave_value, 0.99, wave_value); 
	
	// 将HSV颜色转化为RGB表示
	vec3 rgb_color = hsv2rgb(hsv_color);
	rgb_color*=brightness;
	float alpha=COLOR.a;
	// 最终颜色根据TEXTURE_RECT提供的alpha通道混合背景
	COLOR = vec4(rgb_color, texture(TEXTURE, UV).a*alpha);
}



//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}

shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;

const int MAX_GHOST = 5;

uniform vec2 offset_px[MAX_GHOST];
uniform vec4 ghost_color[MAX_GHOST] : source_color;
uniform int ghost_count = 3;

void fragment()
{
    vec2 uv = SCREEN_UV;
    vec2 px = SCREEN_PIXEL_SIZE;

    vec4 main_col = texture(screen_texture, uv);

    float mask = texture(TEXTURE, UV).a;
	
    vec3 base_factor = COLOR.rgb * COLOR.a * mask;

    vec3 ghost_sum = vec3(0.0);

    for (int i = 0; i < MAX_GHOST; i++)
    {
        if (i >= ghost_count) break;

        vec2 uv2 = uv + offset_px[i] * px;

        vec4 sample = texture(screen_texture, uv2);

        ghost_sum += sample.rgb * ghost_color[i].rgb * ghost_color[i].a * base_factor;
    }

    COLOR = vec4(
        main_col.rgb + ghost_sum,
        main_col.a
    );
}
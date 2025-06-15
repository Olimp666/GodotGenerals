shader_type canvas_item;
uniform vec4 uniform_arr;

void fragment() {
    COLOR = texture(TEXTURE, UV)*tile_custom_data(0);
}
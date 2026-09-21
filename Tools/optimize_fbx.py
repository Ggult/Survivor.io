import bpy
import os
import sys

source = sys.argv[-2]
destination = sys.argv[-1]
ratio = 0.5

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=source, use_anim=False)
mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']
if not mesh_objects:
    raise RuntimeError('No mesh objects found in ' + source)

before = sum(sum(len(poly.vertices) - 2 for poly in obj.data.polygons) for obj in mesh_objects)
for obj in mesh_objects:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    modifier = obj.modifiers.new(name='MobileDecimate', type='DECIMATE')
    modifier.decimate_type = 'COLLAPSE'
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True
    while obj.modifiers.find(modifier.name) > 0:
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_move_up(modifier=modifier.name)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)

after = sum(sum(len(poly.vertices) - 2 for poly in obj.data.polygons) for obj in mesh_objects)
os.makedirs(os.path.dirname(destination), exist_ok=True)
for obj in bpy.context.scene.objects:
    obj.select_set(obj.type in {'MESH', 'ARMATURE'})
bpy.context.view_layer.objects.active = mesh_objects[0]
bpy.ops.export_scene.fbx(
    filepath=destination,
    use_selection=True,
    object_types={'MESH', 'ARMATURE'},
    use_mesh_modifiers=False,
    add_leaf_bones=False,
    bake_anim=False,
    apply_unit_scale=True,
    axis_forward='-Z',
    axis_up='Y',
    path_mode='AUTO',
    embed_textures=False)
print('SOURCE=' + source)
print('DESTINATION=' + destination)
print('TRIANGLES_BEFORE=' + str(before))
print('TRIANGLES_AFTER=' + str(after))
print('REDUCTION_PERCENT=' + str(round((1.0 - after / before) * 100.0, 2)))
print('MESH_OBJECTS=' + str(len(mesh_objects)))

import bpy
import os
import sys

source = sys.argv[-1]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=source, use_anim=False)
mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']
armatures = [obj for obj in bpy.context.scene.objects if obj.type == 'ARMATURE']
triangles = 0
for obj in mesh_objects:
    mesh = obj.data
    triangles += sum(len(poly.vertices) - 2 for poly in mesh.polygons)
print('SOURCE=' + source)
print('MESH_OBJECTS=' + str(len(mesh_objects)))
print('ARMATURES=' + str(len(armatures)))
print('BONES=' + str(sum(len(obj.data.bones) for obj in armatures)))
print('TRIANGLES=' + str(triangles))
for obj in mesh_objects:
    print('MESH=' + obj.name + ';TRIANGLES=' + str(sum(len(poly.vertices) - 2 for poly in obj.data.polygons)) + ';MATERIALS=' + str(len(obj.data.materials)) + ';ARMATURE_MODIFIERS=' + str(sum(1 for modifier in obj.modifiers if modifier.type == 'ARMATURE')))

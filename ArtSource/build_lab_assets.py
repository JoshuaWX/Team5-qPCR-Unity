"""Original Team 5 assets. Run with Blender 5.2 --background --python this_file.
All dimensions are metres. The PCR reference folder is never written to.
"""
import bpy
import math
from mathutils import Vector
from pathlib import Path

PROJECT = Path(__file__).resolve().parent.parent
OUT = PROJECT / 'Assets/Team5/Models/Redesign'
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system = 'METRIC'
bpy.context.scene.unit_settings.scale_length = 1

def material(name, color, metal=0, rough=.4):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Base Color'].default_value = (*color, 1)
    shader.inputs['Metallic'].default_value = metal
    shader.inputs['Roughness'].default_value = rough
    return mat

WHITE = material('Porcelain_Polymer', (.84,.88,.92), 0, .3)
BLUE = material('Clinical_Blue', (.035,.17,.46), .05, .32)
STEEL = material('Brushed_Stainless', (.53,.59,.65), .85, .28)
DARK = material('Graphite_Rubber', (.025,.036,.05), 0, .68)
SCREEN = material('Display_Glass', (.045,.09,.16), .2, .14)
CLEAR = material('Optical_Polypropylene', (.79,.88,.93), 0, .19)
SKIN = material('Skin', (.35,.16,.085), 0, .52)
HAIR = material('Hair', (.025,.015,.012), 0, .8)
COAT = material('Cotton_Coat', (.90,.92,.93), 0, .78)
GLOVE = material('Nitrile_Blue', (.10,.36,.64), 0, .56)
PANTS = material('Navy_Trousers', (.035,.055,.09), 0, .82)

def root(name):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    return obj

def finish(obj, name, parent, mat):
    obj.name = name
    obj.parent = parent
    obj.data.materials.append(mat)
    return obj

def box(name, pos, size, parent, mat, bevel=.003):
    bpy.ops.mesh.primitive_cube_add(size=1, location=pos)
    obj = bpy.context.object
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    finish(obj, name, parent, mat)
    if bevel:
        mod = obj.modifiers.new('Manufactured_edge_radius', 'BEVEL')
        mod.width = bevel
        mod.segments = 3
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
        obj.modifiers.new('Weighted_normals', 'WEIGHTED_NORMAL')
    return obj

def sphere(name, pos, size, parent, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, location=pos)
    obj=bpy.context.object
    obj.scale=size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    finish(obj,name,parent,mat)
    for p in obj.data.polygons: p.use_smooth=True
    return obj

def cylinder(name,pos,radius,depth,parent,mat,vertices=24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=pos)
    obj=finish(bpy.context.object,name,parent,mat)
    mod=obj.modifiers.new('Edge_radius','BEVEL');mod.width=min(radius*.1,.002);mod.segments=2
    bpy.ops.object.modifier_apply(modifier=mod.name)
    for p in obj.data.polygons:p.use_smooth=True
    return obj

def torus(name,pos,major,minor,parent,mat):
    bpy.ops.mesh.primitive_torus_add(major_segments=20,minor_segments=6,location=pos,major_radius=major,minor_radius=minor)
    return finish(bpy.context.object,name,parent,mat)

def export(obj,name,animations=False):
    # One static mesh per material, one skinned mesh for the learner. Keep bone weights.
    meshes = [child for child in obj.children_recursive if child.type == 'MESH']
    groups = {'skin': meshes} if animations else {}
    if not animations:
        for mesh in meshes:
            key = mesh.data.materials[0].name
            groups.setdefault(key, []).append(mesh)
    for key, group in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for mesh in group:
            bpy.context.view_layer.objects.active = mesh
            for mod in list(mesh.modifiers):
                if mod.type != 'ARMATURE': bpy.ops.object.modifier_apply(modifier=mod.name)
            mesh.select_set(True)
        bpy.context.view_layer.objects.active=group[0]
        bpy.ops.object.join()
        bpy.context.object.name = name + '_' + key
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    for child in obj.children_recursive:child.select_set(True)
    bpy.context.view_layer.objects.active=obj
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')), use_selection=True,
        object_types={'EMPTY','MESH','ARMATURE'},apply_unit_scale=True,global_scale=1,
        axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=animations,
        bake_anim_use_all_actions=animations,bake_anim_use_nla_strips=False,
        bake_anim_simplify_factor=0,mesh_smooth_type='FACE')

plate=root('Plate_127_76mm')
box('Skirt', (0,0,.005),(.12776,.08548,.010),plate,WHITE,.002)
box('Top_deck',(0,0,.011),(.125,.083,.003),plate,WHITE,.001)
for row in range(8):
    for col in range(12):
        x=-.0495+col*.009;y=-.0315+row*.009
        cylinder(f'Cavity_{row}_{col}',(x,y,.0127),.0032,.0006,plate,DARK,16)
        torus(f'Well_rim_{row}_{col}',(x,y,.0132),.0032,.00042,plate,CLEAR)
box('Identification_label',(0,-.0415,.009),(.07,.0004,.005),plate,WHITE,.0001)
export(plate,'Clinical_Plate')

instrument=root('RealTime_qPCR_Housing')
box('Enclosure',(0,0,.235),(.45,.48,.43),instrument,WHITE,.024)
box('Blue_base_band',(0,0,.045),(.452,.482,.06),instrument,BLUE,.012)
box('Lid_seam',(0,.01,.45),(.41,.42,.006),instrument,DARK,.003)
box('Lid',(0,.01,.462),(.414,.424,.02),instrument,WHITE,.008)
box('Touchscreen_recess',(0,-.243,.30),(.292,.010,.178),instrument,DARK,.009)
box('Touchscreen_display',(0,-.250,.30),(.266,.003,.15),instrument,SCREEN,.005)
box('Drawer_recess',(0,-.244,.113),(.176,.009,.063),instrument,DARK,.006)
for side in [-1,1]:
    for i in range(12):
        box('Cooling_vent',(side*.226,.02+i*.013,.24),(.001,.004,.12),instrument,DARK,.001)
    for y in [-.17,.17]:cylinder('Rubber_foot',(side*.17,y,.012),.025,.024,instrument,DARK)
export(instrument,'Clinical_Instrument')

drawer=root('Thermal_Drawer')
box('Tray',(0,0,0),(.155,.118,.014),drawer,STEEL,.003)
box('Thermal_contact',(0,0,.009),(.130,.087,.006),drawer,DARK,.001)
box('Front_grip',(0,-.067,-.002),(.174,.022,.048),drawer,WHITE,.006)
box('Finger_recess',(0,-.079,-.007),(.096,.003,.020),drawer,BLUE,.003)
export(drawer,'Clinical_Drawer')

pipette=root('Mechanical_Pipette_200uL')
sphere('Ergonomic_body',(0,0,.115),(.017,.018,.065),pipette,WHITE)
cylinder('Tip_cone',(0,0,.031),.0045,.085,pipette,CLEAR)
cylinder('Stem',(0,0,.174),.005,.03,pipette,STEEL)
cylinder('Plunger',(0,0,.194),.011,.012,pipette,BLUE)
box('Volume_window',(0,-.016,.136),(.019,.002,.024),pipette,DARK,.002)
box('Finger_hook',(0,.015,.151),(.024,.036,.009),pipette,BLUE,.004)
export(pipette,'Clinical_Pipette')

tube=root('Microtube_1_5mL')
cylinder('Tube_body',(0,0,.018),.0053,.028,tube,CLEAR)
bpy.ops.mesh.primitive_cone_add(vertices=24,radius1=.001,radius2=.0053,depth=.012,location=(0,0,.001))
finish(bpy.context.object,'Conical_tip',tube,CLEAR)
cylinder('Hinged_cap',(0,0,.034),.0065,.003,tube,BLUE)
box('Hinge',(0,.005,.031),(.003,.003,.006),tube,BLUE,.001)
export(tube,'Clinical_Microtube')

cabinet=root('Clinical_base_cabinet')
box('Carcass',(0,0,.44),(1.04,.88,.82),cabinet,WHITE,.007)
box('Toe_kick',(0,.02,.055),(.97,.79,.11),cabinet,DARK,.004)
for x in [-.259,.259]:
    for z,h in [(.24,.30),(.55,.29),(.765,.125)]:
        box('Drawer_front',(x,-.449,z),(.504,.022,h),cabinet,WHITE,.004)
        box('Pull',(x,-.478,z+h*.30),(.25,.016,.016),cabinet,STEEL,.006)
export(cabinet,'Clinical_Cabinet')

sink=root('Stainless_washbasin')
box('Basin_shadow',(0,0,.005),(.51,.39,.012),sink,DARK,.035)
box('Basin_floor',(0,0,.012),(.45,.33,.01),sink,STEEL,.025)
for x in [-.28,.28]:box('Side_rim',(x,0,.03),(.05,.47,.04),sink,STEEL,.012)
for y in [-.22,.22]:box('End_rim',(0,y,.03),(.61,.05,.04),sink,STEEL,.012)
cylinder('Drain',(0,0,.02),.035,.002,sink,DARK)
cylinder('Drain_grate',(0,0,.022),.029,.002,sink,STEEL)
cylinder('Faucet_base',(0,.26,.025),.038,.04,sink,STEEL)
cylinder('Tap_riser',(0,.26,.15),.018,.26,sink,STEEL)
box('Tap_spout',(0,.17,.28),(.037,.20,.035),sink,STEEL,.016)
cylinder('Aerator',(0,.074,.26),.016,.04,sink,STEEL)
box('Lever',(0,.29,.31),(.025,.10,.012),sink,STEEL,.005)
export(sink,'Clinical_Sink')

coat=root('Hanging_lab_coat')
box('Coat_body',(0,0,.54),(.34,.075,.88),coat,COAT,.06)
for sign in [-1,1]:
    sleeve=sphere('Hanging_sleeve',(sign*.22,0,.68),(.067,.042,.27),coat,COAT)
    sleeve.rotation_euler.y=sign*.16
    box('Pocket',(sign*.09,-.044,.39),(.08,.009,.10),coat,WHITE,.005)
    box('Collar',(sign*.052,-.05,.92),(.05,.025,.12),coat,WHITE,.008)
for z in [.40,.53,.66,.79]:sphere('Button',(0,-.043,z),(.006,.004,.006),coat,DARK)
export(coat,'Clinical_HangingCoat')

centrifuge=root('Compact_plate_centrifuge')
box('Housing',(0,0,.13),(.48,.38,.26),centrifuge,WHITE,.04)
box('Base',(0,0,.027),(.48,.38,.05),centrifuge,BLUE,.02)
box('Lid_seam',(0,.015,.261),(.42,.31,.005),centrifuge,DARK,.012)
box('Lid',(0,.015,.268),(.42,.31,.01),centrifuge,WHITE,.012)
box('Display',(0,-.191,.16),(.16,.003,.05),centrifuge,SCREEN,.006)
cylinder('Start_key',(.14,-.192,.12),.013,.006,centrifuge,BLUE).rotation_euler.x=math.pi/2
export(centrifuge,'Clinical_Centrifuge')

# A restrained, original mannequin-style human with a real bone hierarchy and baked actions.
scientist=root('Laboratory_Scientist')
bpy.ops.object.armature_add(location=(0,0,0))
rig=bpy.context.object;rig.name='Scientist_Rig';rig.parent=scientist
bpy.ops.object.mode_set(mode='EDIT')
rig.data.edit_bones.remove(rig.data.edit_bones[0])
bone_specs=[('Hips',(0,0,.88),(0,0,1.05),None),('Spine',(0,0,1.05),(0,0,1.35),'Hips'),
 ('Chest',(0,0,1.35),(0,0,1.50),'Spine'),('Neck',(0,0,1.50),(0,0,1.56),'Chest'),
 ('Head',(0,0,1.56),(0,0,1.76),'Neck')]
for side,sign in [('L',1),('R',-1)]:
    bone_specs.extend([(f'UpperArm_{side}',(sign*.20,0,1.46),(sign*.26,0,1.19),'Chest'),
      (f'Forearm_{side}',(sign*.26,0,1.19),(sign*.27,-.015,.97),f'UpperArm_{side}'),
      (f'Hand_{side}',(sign*.27,-.015,.97),(sign*.27,-.02,.86),f'Forearm_{side}'),
      (f'Thigh_{side}',(sign*.10,0,.92),(sign*.10,0,.52),'Hips'),
      (f'Shin_{side}',(sign*.10,0,.52),(sign*.10,0,.12),f'Thigh_{side}'),
      (f'Foot_{side}',(sign*.10,0,.12),(sign*.10,-.15,.07),f'Shin_{side}')])
for name,head,tail,parent in bone_specs:
    b=rig.data.edit_bones.new(name);b.head=head;b.tail=tail
    if parent:b.parent=rig.data.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT')

def skin(obj,bone):
    # Full-weight rigid segments retain a compact mesh; overlapping sleeves cover joints.
    obj.parent=rig
    group=obj.vertex_groups.new(name=bone);group.add(list(range(len(obj.data.vertices))),1,'REPLACE')
    mod=obj.modifiers.new('Scientist_skin','ARMATURE');mod.object=rig
    return obj

skin(sphere('Coat_torso',(0,0,1.29),(.225,.127,.265),scientist,COAT),'Spine')
skin(box('Coat_skirt',(0,0,.94),(.38,.235,.34),scientist,COAT,.05),'Hips')
skin(box('Blue_shirt',(0,-.118,1.43),(.10,.025,.16),scientist,BLUE,.012),'Chest')
for sign in [-1,1]:
    skin(box('Coat_lapel',(sign*.058,-.134,1.425),(.055,.02,.19),scientist,WHITE,.008),'Chest')
    skin(box('Pocket',(sign*.105,-.126,1.16),(.09,.012,.082),scientist,COAT,.008),'Spine')
skin(box('Name_badge',(.105,-.142,1.42),(.075,.012,.045),scientist,BLUE,.004),'Chest')
for z in [1.12,1.23,1.34]:skin(sphere('Coat_button',(0,-.138,z),(.005,.004,.005),scientist,DARK),'Spine')
skin(cylinder('Neck',(0,0,1.55),.041,.08,scientist,SKIN),'Neck')
skin(sphere('Head',(0,-.002,1.661),(.084,.077,.108),scientist,SKIN),'Head')
skin(sphere('Hair',(0,.012,1.715),(.085,.073,.057),scientist,HAIR),'Head')
skin(sphere('Nose',(0,-.076,1.654),(.016,.020,.025),scientist,SKIN),'Head')
for sign in [-1,1]:
    skin(sphere('Ear',(sign*.081,0,1.65),(.012,.013,.026),scientist,SKIN),'Head')
    skin(sphere('Eye',(sign*.032,-.070,1.681),(.016,.007,.009),scientist,WHITE),'Head')
    skin(sphere('Iris',(sign*.032,-.077,1.680),(.005,.002,.005),scientist,DARK),'Head')
    side='L' if sign==1 else 'R'
    skin(sphere('Coat_upper_sleeve',(sign*.235,0,1.325),(.070,.077,.168),scientist,COAT),f'UpperArm_{side}')
    skin(sphere('Coat_fore_sleeve',(sign*.265,0,1.09),(.052,.060,.143),scientist,COAT),f'Forearm_{side}')
    skin(sphere('Gloved_hand',(sign*.27,-.015,.91),(.039,.025,.065),scientist,GLOVE),f'Hand_{side}')
    skin(sphere('Gloved_thumb',(sign*.237,-.028,.93),(.014,.017,.03),scientist,GLOVE),f'Hand_{side}')
    skin(sphere('Trouser_thigh',(sign*.10,0,.71),(.085,.095,.235),scientist,PANTS),f'Thigh_{side}')
    skin(sphere('Trouser_shin',(sign*.10,0,.32),(.065,.070,.215),scientist,PANTS),f'Shin_{side}')
    skin(box('Closed_shoe',(sign*.10,-.055,.06),(.13,.27,.11),scientist,DARK,.045),f'Foot_{side}')

rig.animation_data_create()
for action_name in ['Idle','Walk','Reach']:
    action=bpy.data.actions.new(action_name)
    rig.animation_data.action=action
    for frame in range(1,49,4):
        phase=(frame-1)/48*2*math.pi
        for pb in rig.pose.bones:
            pb.rotation_mode='XYZ';pb.rotation_euler=(0,0,0)
        rig.pose.bones['Spine'].rotation_euler.x=.008*math.sin(phase)
        if action_name=='Walk':
            for side,sign in [('L',1),('R',-1)]:
                rig.pose.bones[f'Thigh_{side}'].rotation_euler.x=sign*.43*math.sin(phase)
                rig.pose.bones[f'Shin_{side}'].rotation_euler.x=max(0,-sign*math.sin(phase))*.6
                rig.pose.bones[f'UpperArm_{side}'].rotation_euler.x=-sign*.24*math.sin(phase)
                rig.pose.bones[f'Forearm_{side}'].rotation_euler.x=-.10
        if action_name=='Reach':
            amount=math.sin((frame-1)/48*math.pi)
            rig.pose.bones['UpperArm_R'].rotation_euler.x=-1.1*amount
            rig.pose.bones['Forearm_R'].rotation_euler.x=-.45*amount
        for pb in rig.pose.bones:pb.keyframe_insert('rotation_euler',frame=frame,group=pb.name)
    # A closed endpoint removes loop pops in Idle/Walk.
    bpy.context.scene.frame_set(1)
    for pb in rig.pose.bones:pb.keyframe_insert('rotation_euler',frame=49,group=pb.name)
    action.use_fake_user=True
rig.animation_data.action=bpy.data.actions['Idle']
bpy.context.scene.frame_start=1;bpy.context.scene.frame_end=49;bpy.context.scene.render.fps=24
bpy.context.scene.frame_set(1)
export(scientist,'Laboratory_Scientist',True)
bpy.ops.wm.save_as_mainfile(filepath=str(PROJECT/'ArtSource/Team5_Clinical_Assets.blend'))
print('TEAM5_ASSETS_COMPLETE',OUT)

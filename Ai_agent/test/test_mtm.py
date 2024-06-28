from citybrain.utils.template_matching import match_template_image

template_file = './res/icons/Notifications/BuildingNotificationElectricityFirst.png'
# template_file = './res/icons/Notifications/unconnect.png'


confidence_threshold = 0.51
screen_image_filename='./res/fewshot/Eletricity_lack.png'
# screen_image_filename='./res/fewshot/Unconnected.png'
match_info = match_template_image(screen_image_filename, template_file, debug=True, output_bb=True, save_matches=True,scale='full')
is_paused = match_info[0]['confidence'] >= confidence_threshold
print(match_info)
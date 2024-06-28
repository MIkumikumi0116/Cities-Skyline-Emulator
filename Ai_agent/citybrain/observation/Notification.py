import os
import time

from citybrain.utils.template_matching import match_template_image
from citybrain.config import Config
from citybrain.log import Logger
from citybrain.gameio import IOEnvironment
from citybrain.gameio.lifecycle.ui_control import take_screenshot


config = Config()
logger = Logger()
io_env = IOEnvironment()

class Notification_observer:
    def __init__(self):
        pass
    def _is_Notification(self,template_file='./res/icons/Notifications/BuildingNotificationElectricityFirst.png',
                        confidence_threshold=0.51):
        flag = False
        # template_file = './res/icons/Notifications/unconnect.png'

        # template_file = './res/icons/Notifications/unconnect.png'

        # screen_image_filename = './res/fewshot/Eletricity_lack.png'
        screen_image_filename = take_screenshot(time.time(), include_minimap=False)[0]
        # screen_image_filename='./res/fewshot/Unconnected.png'
        match_info = match_template_image(screen_image_filename, template_file, debug=True, output_bb=True,
                                          save_matches=True, scale='full')
        flag = match_info[0]['confidence'] >= confidence_threshold

        return flag

    '''
    BuildingNotification
    ['EventGainWater', 'NotificationCrime', 'NotificationDead', 'NotificationDirtyWater', 
    'NotificationElectricity', 'NotificationElectricityFirst', 'NotificationElectricityNotConnected', 
    'NotificationFire', 'NotificationFireHazard', 'NotificationGarbage', 'NotificationGarbagefirst', 
    'NotificationGroundPollution', 'NotificationHealthcare', 'NotificationLandfillFull', 'NotificationNoise', 
    'NotificationParkNotConnected', 'NotificationPolice', 'NotificationRoadNotConnected', 'NotificationSewage',
     'NotificationSewageFirst', 'NotificationSomeoneDead', 'NotificationToofewServices',
      'NotificationToofewServicesCritical', 'NotificationWater', 'NotificationWaterFirst', 
      'NotificationWaterNotConnected']
    '''

    def is_Electricity(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'ElectricityFirst.png')) or self._is_Notification(
            os.path.join(template_file + 'Electricity.png'))

    def is_Water(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'WaterFirst.png')) or self._is_Notification(
            os.path.join(template_file + 'Water.png'))

    def is_WaterNotConnected(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'WaterNotConnected.png'))

    def is_Sewage(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'SewageFirst.png')) or self._is_Notification(
            os.path.join(template_file + 'Sewage.png'))

    def is_ToofewServices(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'ToofewServices.png')) or self._is_Notification(
            os.path.join(template_file + 'ToofewServicesCritical.png'))

    def is_Crime(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'Crime.png')) or self._is_Notification(
            os.path.join(template_file + 'Police.png'))

    def is_Dead(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'SomeoneDead.png')) or self._is_Notification(
            os.path.join(template_file + 'Dead.png'))

    def is_Fire(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'Fire.png'))

    def is_Garbage(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'Garbage.png')) or self._is_Notification(
            os.path.join(template_file + 'Garbagefirst.png'))

    def is_DirtyWater(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'DirtyWater.png'))

    def is_Healthcare(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'Healthcare.png'))

    def is_Noise(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'Noise.png'))

    def is_ParkNotConnected(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'ParkNotConnected.png'))

    def is_RoadNotConnected(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'RoadNotConnected.png'))

    '''check'''

    def is_ElectricityNotConnected(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'DirtyWater.png'))

    def is_FireHazard(self):
        template_file = './res/icons/Notifications/BuildingNotification'
        return self._is_Notification(os.path.join(template_file + 'FireHazard.png'))

    '''
    BuildingEvent
    '''

    def is_GainWater(self):
        template_file = './res/icons/Notifications/BuildingEvent'
        return self._is_Notification(os.path.join(template_file + 'GainWater.png'))






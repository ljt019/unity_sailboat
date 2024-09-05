import network
import socket
from machine import ADC, Pin, PWM
import utime

# Wi-Fi credentials
SSID = "Sci-Fi"
PASSWORD = "Redriver820!"

# UDP settings
UDP_IP = "192.168.1.42"
UDP_PORT = 3030

# Pin configurations
LED_PIN = 16
ADC_PIN = 26

# Deadzone settings
DEADZONE_MIN = 32000
DEADZONE_MAX = 33535

def connect_wifi():
    wlan = network.WLAN(network.STA_IF)
    wlan.active(True)
    if not wlan.isconnected():
        print("Connecting to WiFi...")
        wlan.connect(SSID, PASSWORD)
        while not wlan.isconnected():
            pass
    print("Connected to WiFi")
    print(f"IP address: {wlan.ifconfig()[0]}")
    return wlan

def setup_hardware():
    adc = ADC(Pin(ADC_PIN))
    pwm = PWM(Pin(LED_PIN))
    pwm.freq(1000)  # Set PWM frequency to 1kHz
    return adc, pwm

def map_value(x, in_min, in_max, out_min, out_max):
    return int((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min)

def calculate_led_brightness(adc_value):
    if DEADZONE_MIN <= adc_value <= DEADZONE_MAX:
        return 0
    elif adc_value < DEADZONE_MIN:
        return map_value(adc_value, 0, DEADZONE_MIN, 65535, 0)
    else:
        return map_value(adc_value, DEADZONE_MAX, 65535, 0, 65535)

def main():
    wlan = connect_wifi()
    adc, pwm = setup_hardware()
    
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    last_update = utime.ticks_ms()
    update_interval = 10  # 10ms interval
    
    while True:
        current_time = utime.ticks_ms()
        if utime.ticks_diff(current_time, last_update) >= update_interval:
            # Read ADC value
            adc_value = adc.read_u16()
            
            # Calculate LED brightness based on ADC value
            led_brightness = calculate_led_brightness(adc_value)
            
            # Set PWM duty cycle for LED
            pwm.duty_u16(led_brightness)
            
            # Send ADC value over UDP
            try:
                sock.sendto(adc_value.to_bytes(2, 'little'), (UDP_IP, UDP_PORT))
                print(f"Sent ADC value: {adc_value}, LED brightness: {led_brightness}")
            except Exception as e:
                print(f"Failed to send UDP packet: {e}")
            
            last_update = current_time
        
        utime.sleep_ms(1)  # Small delay to prevent tight looping

if __name__ == "__main__":
    main()
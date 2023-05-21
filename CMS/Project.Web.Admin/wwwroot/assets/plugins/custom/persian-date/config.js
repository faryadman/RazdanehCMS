const datePickerConfig = {
    "inline": false,
    "format": "YYYY/MM/DD",
    "viewMode": "day",
    "initialValue": false,
    "minDate": null,
    "maxDate": null,
    "autoClose": true,
    "position": "auto",
    "altFormat": "YYYY/MM/DD",
    "altField": "#altfieldExample",
    "onlyTimePicker": false,
    "onlySelectOnDate": true,
    "calendarType": "persian",
    "inputDelay": 800,
    "observer": true,
    "calendar": {
        "persian": {
            "locale": "fa",
            "showHint": true,
            "leapYearMode": "algorithmic"
        },
        "gregorian": {
            "locale": "en",
            "showHint": true
        }
    },
    "navigator": {
        "enabled": true,
        "scroll": {
            "enabled": true
        },
        "text": {
            "btnNextText": "<",
            "btnPrevText": ">"
        }
    },
    "toolbox": {
        "enabled": false,
        "calendarSwitch": {
            "enabled": false,
            "format": "LL"
        },
        "todayButton": {
            "enabled": false,
            "text": {
                "fa": "امروز",
                "en": "Today"
            }
        },
        "submitButton": {
            "enabled": false,
            "text": {
                "fa": "تایید",
                "en": "Submit"
            }
        },
        "text": {
            "btnToday": "امروز"
        }
    },
    "timePicker": {
        "enabled": false,
        "step": 1,
        "hour": {
            "enabled": false,
            "step": null
        },
        "minute": {
            "enabled": false,
            "step": null
        },
        "second": {
            "enabled": false,
            "step": null
        },
        "meridian": {
            "enabled": false
        }
    },
    "dayPicker": {
        "enabled": true,
        "titleFormat": "YYYY MMMM"
    },
    "monthPicker": {
        "enabled": true,
        "titleFormat": "YYYY"
    },
    "yearPicker": {
        "enabled": true,
        "titleFormat": "YYYY"
    },
    "responsive": true
};
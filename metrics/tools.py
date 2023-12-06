from datetime import datetime
from typing import Optional
import pandas


def time_to_seconds(time: datetime.time):
    return (time.hour * 60 + time.minute) * 60 + time.second


def convert_field_to_seconds(datatable: pandas.DataFrame, field: Optional[str] = None):
    datatable = datatable.copy()
    if field is None:
        for col in datatable.columns:
            # Pas de functie toe op de gehele kolom
            datatable[f"{col}_seconds"] = datatable[col].apply(time_to_seconds)
    else:
        datatable[f"{field}_seconds"] = datatable[field].apply(time_to_seconds)

    return datatable

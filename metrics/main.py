import pandas as pd
from other_analysis import round_2_files_per_user_touched
from screencast_analysis_setup import build_charts_for_screencast_analysis

screencast_analysis = r"/Users/quintenmeijboom/OneDrive - Avans Hogeschool/Stage/Onderzoek/screencast_analysis.xlsx"

screencast_data = pd.ExcelFile(screencast_analysis)

build_charts_for_screencast_analysis(screencast_data)
round_2_files_per_user_touched(screencast_data)


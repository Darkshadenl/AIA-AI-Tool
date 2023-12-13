from datetime import time
import pandas as pd
from pandas import ExcelFile
from diff_graphs_screencast import diff_time_to_completion, diff_time_building_context_plus_searchtime, \
    diff_amount_changed_correct, diff_amount_changed_wrong, diff_building_context
from tools import convert_field_to_seconds


def build_charts_for_screencast_analysis(screencast_data: ExcelFile):
    ronde_1_data = pd.read_excel(screencast_data, sheet_name='metrics', header=1, nrows=12)
    ronde_2_data = pd.read_excel(screencast_data, sheet_name='metrics', skiprows=38, nrows=10)

    colnames = ['name', 'time_finding_comment', 'time_understand_comment_and_code', 'time_edit_comment']
    before_ai_data = pd.read_excel(screencast_data, sheet_name='metrics', usecols=colnames, skiprows=19, nrows=12)
    ai_data = pd.read_excel(screencast_data, sheet_name='metrics', usecols=colnames, skiprows=54, nrows=11)
    # hierboven staat: neem sheet metrics, skip n rows (skiprows), header row is n omlaag (header), neem n rows (nrows)

    # Filter de DataFrame door alleen rijen te behouden waar kolom 1 niet NaN is
    cleaned_ronde_2_data = ronde_2_data[ronde_2_data.iloc[:, 1].notna()]

    # Selecteer alle namen en converteer naar array
    cl0 = cleaned_ronde_2_data.loc[:, 'name'].to_numpy()

    # soort inner join op naam.
    cleaned_ronde_1_data = ronde_1_data[ronde_1_data.iloc[:, 0].isin(cl0)]
    # op dit moment hebben we alleen data van de 2 tabellen als beide tabellen de naam MET data bevatten

    cleaned_ronde_1_data.set_index('name', inplace=True)
    cleaned_ronde_2_data.set_index('name', inplace=True)

    # Filter de DataFrame door alleen rijen te behouden waar kolom 1 niet NaN is
    cleaned_ai_data = ai_data[ai_data.iloc[:, 1].notna()]
    cleaned_ai_data = cleaned_ai_data[~ai_data.loc[:, 'time_finding_comment'].isin([time(0, 0)])]

    # Selecteer alle namen en converteer naar array
    cl1 = ai_data.loc[:, 'name'].to_numpy()

    # soort inner join op naam.
    cleaned_before_ai_data = before_ai_data[before_ai_data.iloc[:, 0].isin(cl0)]
    # op dit moment hebben we alleen data van de 2 tabellen als beide tabellen de naam MET data bevatten

    cleaned_before_ai_data.set_index('name', inplace=True)
    cleaned_ai_data.set_index('name', inplace=True)

    # converteer datetime.time naar secondes.
    cleaned_before_ai_data = convert_field_to_seconds(cleaned_before_ai_data)
    cleaned_ai_data = convert_field_to_seconds(cleaned_ai_data)

    # draw graphs
    diff_amount_changed_correct(cleaned_ronde_1_data, cleaned_ronde_2_data)
    diff_amount_changed_wrong(cleaned_ronde_1_data, cleaned_ronde_2_data)
    diff_building_context(cleaned_before_ai_data, cleaned_ai_data)
    diff_time_building_context_plus_searchtime(cleaned_before_ai_data, cleaned_ai_data)
    diff_time_to_completion(cleaned_before_ai_data, cleaned_ai_data)

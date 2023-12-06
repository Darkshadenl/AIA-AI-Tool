import pandas as pd
import matplotlib.pyplot as plt


def diff_amount_changed_correct(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    combined_data = pd.merge(pd_one[['amount_changed_correct']],
                             pd_two[['amount_changed_correct']],
                             on='name',
                             suffixes=('_ronde_1', '_ronde_2'))

    combined_data.plot(kind='bar', figsize=(10, 6))
    plt.title('Vergelijking van Amount Changed Correct tussen Ronde 1 en Ronde 2')
    plt.ylabel('Amount Changed Correct')
    plt.xlabel('Name')
    plt.show()


def diff_amount_changed_wrong(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    combined_data = pd.merge(pd_one[['amount_changed_wrong']],
                             pd_two[['amount_changed_wrong']],
                             on='name',
                             suffixes=('_ronde_1', '_ronde_2'))
    combined_data.plot(kind='bar', figsize=(10, 6))
    plt.title('Vergelijking van Amount Changed Wrong tussen Ronde 1 en Ronde 2')
    plt.ylabel('Amount Changed Wrong')
    plt.xlabel('Name')
    plt.ylim(0, 10)
    plt.show()


def diff_building_context(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    combined_data = pd.merge(pd_one[['time_understand_comment_and_code_seconds']],
                             pd_two[['time_understand_comment_and_code_seconds']],
                             on='name',
                             suffixes=('_ronde_1', '_ronde_2'))

    combined_data.plot(kind='bar', figsize=(10, 6))
    plt.title('Vergelijking van Time To Understand Comments and Code tussen Ronde 1 en Ronde 2')
    plt.ylabel('Time To Understand Comments and Code (in seconds)')
    plt.xlabel('Name')
    plt.ylim(0, 1000)
    plt.show()


def diff_time_building_context_plus_searchtime(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    pd_one['total_time_seconds'] = pd_one['time_finding_comment_seconds'] + pd_one[
        'time_understand_comment_and_code_seconds']
    pd_two['total_time_seconds'] = pd_two['time_finding_comment_seconds'] + pd_two[
        'time_understand_comment_and_code_seconds']

    combined_data = pd.merge(pd_one[['total_time_seconds']],
                             pd_two[['total_time_seconds']],
                             on='name',
                             suffixes=('_ronde_1', '_ronde_2'))

    combined_data.plot(kind='bar', figsize=(10, 6))
    plt.title('Vergelijking Time to build context plus searchtime tussen Ronde 1 en Ronde 2')
    plt.ylabel('Time to build context plus searchtime (in seconds)')
    plt.xlabel('Name')
    plt.ylim(0, 1000)
    plt.show()


def diff_time_to_completion_with_ai_time(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    pass


def diff_amount_faulty_missed(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    pass

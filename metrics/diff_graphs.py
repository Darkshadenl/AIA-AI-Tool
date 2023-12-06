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
    pass


def diff_building_context_and_searchtime(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    pass


def diff_time_to_completion(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    pass


def diff_time_to_completion_with_ai_time(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    pass


def diff_amount_faulty_missed(pd_one: pd.DataFrame, pd_two: pd.DataFrame):
    pass

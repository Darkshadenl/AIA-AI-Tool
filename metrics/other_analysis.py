import pandas as pd
from matplotlib import pyplot as plt


def round_2_amount_files_per_user(files_per_participant: dict):
    df = pd.DataFrame(list(files_per_participant.items()), columns=['Participant', 'Files'])
    # Plotting
    df.plot(kind='bar')
    plt.ylabel('Number of files')
    plt.title('The Amount of Files per User')
    plt.show()


def round_2_files_per_user_touched(screencast_data):
    ronde_2_data = pd.read_excel(screencast_data, sheet_name='Analyse_using_ai', header=0, nrows=3, usecols='P:AO')
    participants = 8
    participants_per_file = {}
    files_per_participant = {}
    file_count = 0

    for col in ronde_2_data.columns:
        file_count += 1
        participant_count = participants
        participants_per_file[col] = participant_count
        for row in ronde_2_data[col]:
            if pd.notna(row):
                files_per_participant[row] = file_count
                participant_count -= 1
        participants = participant_count

    round_2_amount_files_per_user(files_per_participant)

    plt.figure(figsize=(10, 6))
    bars = plt.bar(
        x=list(participants_per_file.keys()),
        height=list(participants_per_file.values()),
    )
    padding = -0.2
    for idx, bar in enumerate(bars):
        yval = bar.get_height() + padding
        plt.text(bar.get_x() + bar.get_width() / 2.0, yval, list(participants_per_file.keys())[idx],
                 rotation='vertical', ha='right', va='center', color='white')

    plt.title("Number of people who touched a file")
    plt.xlabel("Person")
    plt.xticks(rotation=0)
    plt.ylabel("Number of people")
    plt.tight_layout()
    plt.show()

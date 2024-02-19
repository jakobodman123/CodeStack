#include <QDialog>
#include <QStackedWidget>
#include <QLineEdit>
#include <QPushButton>
#include <QFileDialog>
#include <QListWidget>
#include <QVBoxLayout>
#include <QHBoxLayout>

class InstallerDialog : public QDialog {
    Q_OBJECT

public:
    InstallerDialog(QWidget *parent = nullptr);

private slots:
    void goToNextStep();
    void goToPreviousStep();
    void chooseFile();
    void finishInstallation();

private:
    QStackedWidget *stackedWidget;
    QLineEdit *nameLineEdit;
    QPushButton *fileChooseButton;
    QString chosenFilePath;
    QListWidget *firstListWidget;
    QListWidget *secondListWidget;
    int currentStep;

    void setupStepOne();
    void setupStepTwo();
    void setupStepThree();
    void setupStepFour();
};

InstallerDialog::InstallerDialog(QWidget *parent) : QDialog(parent), currentStep(0) {
    stackedWidget = new QStackedWidget(this);

    setupStepOne();
    setupStepTwo();
    setupStepThree();
    setupStepFour();

    QVBoxLayout *mainLayout = new QVBoxLayout(this);
    mainLayout->addWidget(stackedWidget);

    QHBoxLayout *buttonLayout = new QHBoxLayout();
    QPushButton *prevButton = new QPushButton("Previous", this);
    QPushButton *nextButton = new QPushButton("Next", this);
    connect(prevButton, &QPushButton::clicked, this, &InstallerDialog::goToPreviousStep);
    connect(nextButton, &QPushButton::clicked, this, &InstallerDialog::goToNextStep);

    buttonLayout->addWidget(prevButton);
    buttonLayout->addWidget(nextButton);

    if (currentStep == 0) {
        prevButton->setDisabled(true);
    }

    mainLayout->addLayout(buttonLayout);

    QPushButton *okButton = new QPushButton("Ok", this);
    connect(okButton, &QPushButton::clicked, this, &InstallerDialog::finishInstallation);
    mainLayout->addWidget(okButton);
}

void InstallerDialog::setupStepOne() {
    QWidget *stepOneWidget = new QWidget(this);
    QVBoxLayout *layout = new QVBoxLayout(stepOneWidget);
    nameLineEdit = new QLineEdit(stepOneWidget);
    layout->addWidget(nameLineEdit);
    stackedWidget->addWidget(stepOneWidget);
}

void InstallerDialog::setupStepTwo() {
    QWidget *stepTwoWidget = new QWidget(this);
    QVBoxLayout *layout = new QVBoxLayout(stepTwoWidget);
    fileChooseButton = new QPushButton("Choose File", stepTwoWidget);
    connect(fileChooseButton, &QPushButton::clicked, this, &InstallerDialog::chooseFile);
    layout->addWidget(fileChooseButton);
    stackedWidget->addWidget(stepTwoWidget);
}

void InstallerDialog::setupStepThree() {
    QWidget *stepThreeWidget = new QWidget(this);
    QVBoxLayout *layout = new QVBoxLayout(stepThreeWidget);
    firstListWidget = new QListWidget(stepThreeWidget);
    // Add items to firstListWidget
    layout->addWidget(firstListWidget);
    stackedWidget->addWidget(stepThreeWidget);
}

void InstallerDialog::setupStepFour() {
    QWidget *stepFourWidget = new QWidget(this);
    QVBoxLayout *layout = new QVBoxLayout(stepFourWidget);
    secondListWidget = new QListWidget(stepFourWidget);
    // Add items to secondListWidget
    layout->addWidget(secondListWidget);
    stackedWidget->addWidget(stepFourWidget);
}

void InstallerDialog::goToNextStep() {
    if (currentStep < 3) {
        currentStep++;
        stackedWidget->setCurrentIndex(currentStep);
    }
}

void InstallerDialog::goToPreviousStep() {
    if (currentStep > 0) {
        currentStep--;
        stackedWidget->setCurrentIndex(currentStep);
    }
}

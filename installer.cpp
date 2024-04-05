class MyDialog : public QDialog {
public:
    MyDialog(QWidget *parent = nullptr) : QDialog(parent) {
        QHBoxLayout *mainLayout = new QHBoxLayout(this);

        // Side panel setup
        QVBoxLayout *sidePanelLayout = new QVBoxLayout;

        // Main content setup
        QStackedWidget *mainContent = new QStackedWidget;

        // Create buttons for the side panel and pages for the main content
        for (int i = 1; i <= 5; ++i) {
            QPushButton *tabButton = new QPushButton(QString("Tab %1").arg(i));
            QLabel *pageLabel = new QLabel(QString("Content for Tab %1").arg(i));

            QWidget *pageWidget = new QWidget; // Container for this page's content
            QVBoxLayout *pageLayout = new QVBoxLayout(pageWidget);
            pageLayout->addWidget(pageLabel);
            pageWidget->setLayout(pageLayout);

            mainContent->addWidget(pageWidget); // Add the page to the stacked widget

            sidePanelLayout->addWidget(tabButton); // Add button to the side panel

            // Connect button click to change the main content
            connect(tabButton, &QPushButton::clicked, [mainContent, i]() {
                mainContent->setCurrentIndex(i - 1); // Indexes are 0-based
            });
        }

        sidePanelLayout->addStretch(1); // Push all buttons to the top

        mainLayout->addLayout(sidePanelLayout);
        mainLayout->addWidget(mainContent, 1); // Main content takes the rest of the space
    }
};



const char* cdCmd = "cd /d C:\\ProgramData\\reconfig";

    // 7-Zip command to zip the folders and file without including the parent directory
    const char* zipCmd = "7z a -tzip output.zip Computer\\* Ca\\* text.csv";

    // Execute the commands
    if (system(cdCmd) == 0 && system(zipCmd) == 0) {
        std::cout << "Zip operation successful." << std::endl;
    } else {
        std::cerr << "Zip operation failed." << std::endl;
    }
#include <cstdlib> // For system()
#include <iostream>

int main() {
    // Change directory to the parent of the folders and file
    int result = system("cd /d C:\\ProgramData\\reconfig");

    // Check if directory change was successful
    if(result != 0) {
        std::cerr << "Failed to change directory. Make sure the path is correct." << std::endl;
        return 1; // Exit if directory change failed
    }

    // Zip the contents without including the parent directory
    result = system("powershell -Command \"Compress-Archive -Path Computer, Ca, text.csv -DestinationPath C:\\ProgramData\\reconfig\\output.zip\"");

    // Check the result of the zip operation
    if(result == 0) {
        std::cout << "Zip operation successful." << std::endl;
    } else {
        std::cerr << "Zip operation failed." << std::endl;
        return 1; // Exit if zip operation failed
    }

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

void YourExistingClass::showInstallerDialog() {
    QDialog *dialog = new QDialog;
    QStackedWidget *stackedWidget = new QStackedWidget(dialog);

    // Step 1: Name input
    QWidget *stepOneWidget = new QWidget;
    QVBoxLayout *stepOneLayout = new QVBoxLayout(stepOneWidget);
    QLineEdit *nameLineEdit = new QLineEdit;
    stepOneLayout->addWidget(nameLineEdit);
    stackedWidget->addWidget(stepOneWidget);

    // Step 2: File selection
    QWidget *stepTwoWidget = new QWidget;
    QVBoxLayout *stepTwoLayout = new QVBoxLayout(stepTwoWidget);
    QPushButton *fileChooseButton = new QPushButton("Choose File");
    QObject::connect(fileChooseButton, &QPushButton::clicked, [dialog]() {
        QFileDialog::getOpenFileName(dialog, "Select File");
    });
    stepTwoLayout->addWidget(fileChooseButton);
    stackedWidget->addWidget(stepTwoWidget);

    // Additional steps (3 and 4) setup...

    QVBoxLayout *mainLayout = new QVBoxLayout(dialog);
    mainLayout->addWidget(stackedWidget);

    // Navigation and action buttons
    QPushButton *prevButton = new QPushButton("Previous");
    QPushButton *nextButton = new QPushButton("Next");
    QPushButton *okButton = new QPushButton("Ok");

    // Connect signals and slots for buttons...

    QHBoxLayout *buttonLayout = new QHBoxLayout;
    buttonLayout->addWidget(prevButton);
    buttonLayout->addWidget(nextButton);
    mainLayout->addLayout(buttonLayout);
    mainLayout->addWidget(okButton);

    // Show the dialog
    dialog->setLayout(mainLayout);
    dialog->exec();
}
std::string basePath = R"(C:\path\to\)"; // Base path where the file and folders are located std::string fileToZip = "example.txt"; // The specific file you want to zip std::string folder1ToZip = "folder1"; // The first folder you want to zip std::string folder2ToZip = "folder2"; // The second folder you want to zip std::string zipPath = R"(C:\path\to\destination\archive.zip)"; // Destination zip file path // Construct the PowerShell command std::string command = "powershell -command \"Compress-Archive -Path '" + basePath + fileToZip + "', '" + basePath + folder1ToZip + "', '" + basePath + folder2ToZip + "' -DestinationPath '" + zipPath + "'\""; // Execute the command

std::string command = "powershell -command \"& {cd '" + folderPath + "'; Compress-Archive -Path .\\* -DestinationPath '" + zipPath + "'}\""
#include <windows.h>
#include <WtsApi32.h>
#include <iostream>

#pragma comment(lib, "Wtsapi32.lib")

BOOL ImpersonateActiveUserAndExecute(std::function<void()> func) {
    DWORD sessionId = WTSGetActiveConsoleSessionId(); // Get the active session ID
    HANDLE hToken = NULL;
    HANDLE hTokenDup = NULL;

    // Obtain the user token for the session
    if (!WTSQueryUserToken(sessionId, &hToken)) {
        std::cerr << "WTSQueryUserToken failed with error " << GetLastError() << std::endl;
        return FALSE;
    }

    // Duplicate the token to get an impersonation token
    if (!DuplicateTokenEx(hToken, MAXIMUM_ALLOWED, NULL, SecurityImpersonation, TokenImpersonation, &hTokenDup)) {
        std::cerr << "DuplicateTokenEx failed with error " << GetLastError() << std::endl;
        CloseHandle(hToken);
        return FALSE;
    }

    // Impersonate the user
    if (!ImpersonateLoggedOnUser(hTokenDup)) {
        std::cerr << "ImpersonateLoggedOnUser failed with error " << GetLastError() << std::endl;
        CloseHandle(hTokenDup);
        CloseHandle(hToken);
        return FALSE;
    }

    // Execute the function as the impersonated user
    func();

    // Revert to self
    RevertToSelf();

    // Cleanup
    CloseHandle(hTokenDup);
    CloseHandle(hToken);

    return TRUE;
}

void AccessUserProfile() {
    DWORD bufferSize = MAX_PATH;
    TCHAR userProfile[MAX_PATH];
    if (GetEnvironmentVariable("USERPROFILE", userProfile, bufferSize)) {
        std::wcout << L"USERPROFILE: " << userProfile << std::endl;
    } else {
        std::cerr << "Failed to get USERPROFILE" << std::endl;
    }
}

int main() {
    ImpersonateActiveUserAndExecute(AccessUserProfile);
    return 0;
}

#include <windows.h>

int main() {
    // Handle for the event log
    HANDLE hEventLog = NULL;

    // Register the event source
    hEventLog = RegisterEventSource(NULL, TEXT("vpntls"));

    if (hEventLog != NULL) {
        // Define your message
        LPCTSTR lpStrings[1] = { TEXT("Example event log message from vpntls.") };

        // Report the event
        if (!ReportEvent(hEventLog,           // Event log handle
                         EVENTLOG_INFORMATION_TYPE, // Event type
                         0,                  // Event category
                         0,                  // Event identifier
                         NULL,               // No user security identifier
                         1,                  // Number of substitution strings
                         0,                  // No data
                         lpStrings,          // Pointer to strings
                         NULL)) {            // No data
            // Handle error
            DWORD dwError = GetLastError();
            // Add error handling code here
        }

        // Deregister the event source
        DeregisterEventSource(hEventLog);
    } else {
        // Handle error if RegisterEventSource fails
        DWORD dwError = GetLastError();
        // Add error handling code here
    }

    return 0;
}


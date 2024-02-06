#include <QMainWindow>
#include <QDialog>
#include <QTableWidget>
#include <QVBoxLayout>
#include <QFile>
#include <QTextStream>
#include <QStringList>
#include <QHeaderView>

class MainWindow : public QMainWindow {
    Q_OBJECT

public:
    MainWindow(QWidget *parent = nullptr) : QMainWindow(parent) {
        setupResponseDialog();
    }

private:
    QDialog* responseDialog;
    QTableWidget* computerTableWidget; // Use QTableWidget instead of QListWidget

    void setupResponseDialog() {
        responseDialog = new QDialog(this);
        responseDialog->setWindowTitle("API Response Data");

        // Initialize the table widget with 0 rows and 2 columns
        computerTableWidget = new QTableWidget(0, 2, responseDialog);
        computerTableWidget->setHorizontalHeaderLabels({"Computer", "Status"}); // Set column headers
        computerTableWidget->horizontalHeader()->setStretchLastSection(true); // Make the last column stretch to fill the table
        computerTableWidget->verticalHeader()->setVisible(false); // Hide the vertical header
        computerTableWidget->setEditTriggers(QAbstractItemView::NoEditTriggers); // Make the table read-only
        computerTableWidget->setSelectionMode(QAbstractItemView::NoSelection); // Disable row selection

        QVBoxLayout* layout = new QVBoxLayout(responseDialog);
        layout->addWidget(computerTableWidget);
        responseDialog->setLayout(layout);
    }

    void onApiRequestButtonClicked() {
        QFile file("path/to/mock_computers.csv"); // Replace with the actual file path
        if (!file.open(QIODevice::ReadOnly | QIODevice::Text))
            return;

        QTextStream in(&file);
        bool firstLine = true;

        computerTableWidget->setRowCount(0); // Clear existing rows

        while (!in.atEnd()) {
            QString line = in.readLine();
            if (firstLine) { // Skip the header line of the CSV
                firstLine = false;
                continue;
            }
            QStringList fields = line.split(',');
            if (fields.size() == 2) { // Ensure each line has exactly 2 fields
                int currentRow = computerTableWidget->rowCount();
                computerTableWidget->insertRow(currentRow);
                computerTableWidget->setItem(currentRow, 0, new QTableWidgetItem(fields[0])); // Computer name
                computerTableWidget->setItem(currentRow, 1, new QTableWidgetItem(fields[1])); // Status
            }
        }

        file.close();
        computerTableWidget->resizeColumnsToContents(); // Adjust column width to content

        if (!responseDialog->isVisible()) {
            responseDialog->show();
        }
    }

    // Method to be called when a specific command is received, triggering the display of CSV data
    void onCommandReceived() {
        onApiRequestButtonClicked(); // This simulates the action triggered by receiving a command
    }
};
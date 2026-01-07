-- MySQL CREATE TABLE statements for Shelf Life Monitoring

CREATE TABLE sample_test (
    id INT AUTO_INCREMENT PRIMARY KEY,
    numrec INT NOT NULL,
    coddes SMALLINT,
    codvar SMALLINT,
    start_date DATETIME NOT NULL,
    initial_fruit_count INT NOT NULL,
    status ENUM('Active', 'Closed') NOT NULL
);

CREATE TABLE daily_check (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sample_test_id INT NOT NULL,
    check_date DATETIME NOT NULL,
    FOREIGN KEY (sample_test_id) REFERENCES sample_test(id) ON DELETE CASCADE
);

CREATE TABLE daily_check_detail (
    id INT AUTO_INCREMENT PRIMARY KEY,
    daily_check_id INT NOT NULL,
    defect_type ENUM('Rot', 'Mold', 'Soft') NOT NULL,
    quantity INT NOT NULL,
    FOREIGN KEY (daily_check_id) REFERENCES daily_check(id) ON DELETE CASCADE
);

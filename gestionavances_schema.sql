-- Dropping existing tables if any
DROP TABLE IF EXISTS `adherent_charges`;
DROP TABLE IF EXISTS `gestionavances_d`;
DROP TABLE IF EXISTS `charges`;
DROP TABLE IF EXISTS `gestionavances`;

-- Main table
CREATE TABLE `gestionavances` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`refadh` INT(11) NULL DEFAULT NULL,
	`date` DATE NULL DEFAULT NULL,
	`annee` INT(11) NULL DEFAULT NULL,
	`mois` INT(11) NULL DEFAULT NULL,
	`ttdecompte` DOUBLE NULL DEFAULT NULL,
	`ttcharges` DOUBLE NULL DEFAULT NULL,
	`tgExport` DOUBLE NULL DEFAULT NULL,
	`prix_esteme_mois` DOUBLE NULL DEFAULT NULL,
	`decaompte_esteme` DOUBLE NULL DEFAULT NULL,
	`s1` DOUBLE NULL DEFAULT NULL,
	`s2` DOUBLE NULL DEFAULT NULL,
	`s3` DOUBLE NULL DEFAULT NULL,
	`s4` DOUBLE NULL DEFAULT NULL,
	`s5` DOUBLE NULL DEFAULT NULL,
	`real_t_s1` DOUBLE NULL DEFAULT NULL,
	`real_t_s2` DOUBLE NULL DEFAULT NULL,
	`real_t_s3` DOUBLE NULL DEFAULT NULL,
	`real_t_s4` DOUBLE NULL DEFAULT NULL,
	`real_t_s5` DOUBLE NULL DEFAULT NULL,
	`real_dec_s1` DOUBLE NULL DEFAULT NULL,
	`real_dec_s2` DOUBLE NULL DEFAULT NULL,
	`real_dec_s3` DOUBLE NULL DEFAULT NULL,
	`real_dec_s4` DOUBLE NULL DEFAULT NULL,
	`real_dec_s5` DOUBLE NULL DEFAULT NULL,
	`montant` DOUBLE NULL DEFAULT NULL,
    PRIMARY KEY (`id`)
) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;

-- Reference table for charges
CREATE TABLE `charges` (
    `idcharge` INT(11) NOT NULL AUTO_INCREMENT,
    `label` VARCHAR(255) NULL DEFAULT NULL,
    `typecharge` VARCHAR(255) NULL DEFAULT NULL,
    PRIMARY KEY (`idcharge`)
) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;


-- Independent Adherent Charges table
CREATE TABLE `adherent_charges` (
    `id` INT(11) NOT NULL AUTO_INCREMENT,
    `refadh` INT(11) NULL DEFAULT NULL,
    `date` DATE NULL DEFAULT NULL,
    `idcharge` INT(11) NULL DEFAULT NULL,
    `montant` DOUBLE NULL DEFAULT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_adherent_charges_charge` FOREIGN KEY (`idcharge`) REFERENCES `charges`(`idcharge`) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT `fk_adherent_charges_adherent` FOREIGN KEY (`refadh`) REFERENCES `adherent`(`refadh`) ON UPDATE CASCADE ON DELETE CASCADE
) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;

-- Schema de base de données pour la bibliothèque
-- Version mise à jour avec toutes les colonnes nécessaires

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

-- --------------------------------------------------------
-- Structure de la table `roles`
-- --------------------------------------------------------

DROP TABLE IF EXISTS `loans`;
DROP TABLE IF EXISTS `users`;
DROP TABLE IF EXISTS `books`;
DROP TABLE IF EXISTS `book_types`;
DROP TABLE IF EXISTS `roles`;

CREATE TABLE `roles` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(60) NOT NULL,
  `access_level` INT NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- access_level: 1=Student (base), 2=Teacher, 3=Librarian/Admin (tout)
INSERT INTO `roles` (`id`, `name`, `access_level`) VALUES
(1, 'Librarian', 3),
(2, 'Student', 1),
(3, 'Teacher', 2);

-- --------------------------------------------------------
-- Structure de la table `book_types` (catégories de livres)
-- --------------------------------------------------------

CREATE TABLE `book_types` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `description` VARCHAR(255) NULL,
  `min_access_level` INT NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Types de livres avec niveaux d'accès
-- min_access_level: 1=tous, 2=Teacher+Admin, 3=Admin seulement
INSERT INTO `book_types` (`id`, `name`, `description`, `min_access_level`) VALUES
(1, 'Étudiants', 'Livres accessibles à tous', 1),
(2, 'Enseignants', 'Livres réservés aux enseignants et administrateurs', 2),
(3, 'Bibliothécaire', 'Livres réservés aux administrateurs/bibliothécaires', 3);

-- --------------------------------------------------------
-- Structure de la table `users`
-- --------------------------------------------------------

CREATE TABLE `users` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `email` VARCHAR(255) NOT NULL UNIQUE,
  `password_hash` VARCHAR(255) NOT NULL,
  `first_name` VARCHAR(60) NOT NULL,
  `last_name` VARCHAR(60) NOT NULL,
  `role_id` INT NOT NULL DEFAULT 2,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`role_id`) REFERENCES `roles`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Utilisateurs de test (mot de passe: "password123" hashe avec BCrypt)
INSERT INTO `users` (`email`, `password_hash`, `first_name`, `last_name`, `role_id`) VALUES
('admin@biblio.fr', '$2b$12$X.hxJOCU4Ae5QGiTSuP3YOeBzZIS2eDpy2n9xSp6NtE3j6zP4InrC', 'Administrateur', '', 1),
('amine.binoumar@etu.fr', '$2b$12$X.hxJOCU4Ae5QGiTSuP3YOeBzZIS2eDpy2n9xSp6NtE3j6zP4InrC', 'Amine', 'Binoumar', 2),
('kilian.godimus@etu.fr', '$2b$12$X.hxJOCU4Ae5QGiTSuP3YOeBzZIS2eDpy2n9xSp6NtE3j6zP4InrC', 'Kilian', 'Godimus', 2),
('dimitri.zavodski@etu.fr', '$2b$12$X.hxJOCU4Ae5QGiTSuP3YOeBzZIS2eDpy2n9xSp6NtE3j6zP4InrC', 'Dimitri', 'Zavodski', 2);

-- --------------------------------------------------------
-- Structure de la table `books`
-- --------------------------------------------------------

CREATE TABLE `books` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `title` VARCHAR(255) NOT NULL,
  `author` VARCHAR(255) NOT NULL,
  `isbn` VARCHAR(20) NULL,
  `description` TEXT NULL,
  `publication_year` INT NULL,
  `availability` ENUM('available', 'borrowed', 'reserved') DEFAULT 'available',
  `image_path` VARCHAR(255) NULL,
  `type_id` INT NOT NULL DEFAULT 1,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`type_id`) REFERENCES `book_types`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Livres de test
INSERT INTO `books` (`title`, `author`, `isbn`, `description`, `publication_year`, `availability`, `type_id`) VALUES
('Clean Code', 'Robert C. Martin', '978-0132350884', 'A Handbook of Agile Software Craftsmanship', 2008, 'available', 1),
('Design Patterns', 'Gang of Four', '978-0201633610', 'Elements of Reusable Object-Oriented Software', 1994, 'available', 2),
('The Pragmatic Programmer', 'Hunt & Thomas', '978-0135957059', 'Your Journey to Mastery', 2019, 'available', 1),
('Refactoring', 'Martin Fowler', '978-0134757599', 'Improving the Design of Existing Code', 2018, 'available', 2),
('Code Complete', 'Steve McConnell', '978-0735619678', 'A Practical Handbook of Software Construction', 2004, 'available', 3);

-- --------------------------------------------------------
-- Structure de la table `loans` (emprunts)
-- --------------------------------------------------------

CREATE TABLE `loans` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `book_id` INT NOT NULL,
  `loan_date` DATE NOT NULL,
  `due_date` DATE NOT NULL,
  `return_date` DATE NULL,
  `status` ENUM('active', 'returned', 'overdue') DEFAULT 'active',
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`),
  FOREIGN KEY (`book_id`) REFERENCES `books`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Pas d'emprunts par défaut, base vide

-- --------------------------------------------------------
-- Structure de la table `reminder_history` (historique des rappels)
-- --------------------------------------------------------

CREATE TABLE `reminder_history` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `loan_id` INT NOT NULL,
  `reminder_type` VARCHAR(20) NOT NULL,
  `sent_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`loan_id`) REFERENCES `loans`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------
-- Structure de la table `notifications` (notifications utilisateur)
-- --------------------------------------------------------

CREATE TABLE `notifications` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `title` VARCHAR(255) NOT NULL,
  `message` TEXT NOT NULL,
  `type` ENUM('reminder', 'overdue', 'info') DEFAULT 'info',
  `is_read` BOOLEAN DEFAULT FALSE,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

# Présentation

Les fichiers dans ce dossier contient les fichiers nécessaires pour implémenter les tests automatiques de DAO/DAL générés par l'outil [Sql Test Generator](https://github.com/klee-contrib/kinetix-tools/tree/develop/Kinetix.Tools.SqlTestGenerator).

# Fichiers

## TestUtil

Publie un provider de services.

Publie une méthode pour initialiser les services.

## TestInitializer.cs

Enregistre le provider de service au démarrage d'une campagne de test.

## DummyValues

Publie une API pour obtenir des valeurs factices à fournir en paramètres des méthodes à tester.

## DalTest

Classe de base pour les tests, publie le DummyValues, gère le scope de transaction.

## DalTestExtensions

Méthode d'extension CheckDalSyntax qui attrape les erreurs post exécution de SQL (violation de contraintes, nombre de lignes renvoyées incorrect etc.).

Elle utilise le provider de service pour instancier le DAO/DAL.

Cette méthode est à adapter en fonction du projet, du système de base de données, de l'ORM etc.

## ExempleTest

Exemple de fichier de test généré par l'outil.

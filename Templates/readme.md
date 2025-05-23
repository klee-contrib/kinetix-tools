# Présentation

Les fichiers dans ce dossier contient les fichiers nécessaires pour implémenter les tests automatiques de DAO/DAL générés par l'outil [Sql Test Generator](https://github.com/klee-contrib/kinetix-tools/tree/develop/Kinetix.Tools.SqlTestGenerator).

# Fichiers

## DummyValues

Publie une API pour obtenir des valeurs factices à fournir en paramètres des méthodes à tester.

## DalTest

Classe de base pour les tests, publie le DummyValues, gère le scope de transaction.

## DalTestExtensions

Méthode d'extension CheckDalSyntax qui attrapents les erreurs post exécution de SQL (violation de contraintes, nombre de lignes renvoyées incorrect etc.).

Cette méthode est à adapter en fonction du projet, du système de base de données, de l'ORM etc.

# Présentation

Ce projet Visual Studio est à copier-coller dans les sources de votre projet.

Il faut adapter : 
- le namepace à renommer avec celui de votre projet
- la gestion du provider de services à tester dans `SharedState.Provider`
- la gestion du scope transactionnel dans `DalTest`
- les données métiers factices dans le dossier `Data`
  - l'idée est de publier des Id statiques ou des bean statiques pour les utiliser en entrée des appels de services à tester
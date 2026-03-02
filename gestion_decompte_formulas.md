# Formules et Logique du Module "Gestion Décompte"

## 1. Tonnages Hebdomadaires (`s1`, `s2`, `s3`, `s4`)
Les colonnes `s1` à `s4` dans la base de données représentent **exclusivement le Tonnage Physique Exporté** (en Kg) au cours des semaines du mois sélectionné.
* `s1` = Total (Kg) exporté la Semaine 1
* `s2` = Total (Kg) exporté la Semaine 2
* `s3` = Total (Kg) exporté la Semaine 3
* `s4` = Total (Kg) exporté la Semaine 4

## 2. Décompte Estimé Total (`decaompte_esteme`)
C'est la valeur globale théorique de la récolte (les "Revenus Bruts").
* **Formule** : Somme pour chaque groupe de variétés de `(Tonnage Total du groupe) * (Prix Estimé du groupe)`
* *Exemple* : Si S1+S2+S3+S4 = 10 000 Kg de Myrtilles à 5 DH/Kg, alors `decaompte_esteme` = 50 000 DH.

## 3. Total des Charges (`ttcharges`)
C'est le cumul de toutes les dettes ou avances financières accordées à l'adhérent jusqu'à la date du décompte.
* **Formule** : Somme de tous les montants dans la table `adherent_charges` pour cet adhérent avant/à la date du décompte.

## 4. Solde Net (Le "Nouveau" `ttdecompte`)
C'est la différence entre ce que l'adhérent gagne et ce qu'il doit. Ce qui lui revient concrètement (ou ce qu'il doit encore si négatif).
* **Formule** : `ttdecompte` = `decaompte_esteme` (Gains bruts) - `ttcharges` (Dettes).
* *Note importante* : Auparavant, l'ancien système appelait cela "Restant Décompte". Aujourd'hui, il remplace directement la valeur de `ttdecompte`.

## 5. Le Champ Montant (`montant`)
Anciennement "Montant Avance" saisi manuellement, il est aujourd'hui automatiquement rempli sur la même valeur thèorique que le montant total estimé (`decaompte_esteme`).

---
**En résumé pour la base de données :**
- `s1`, `s2`, `s3`, `s4` = **Kilos**
- `tgExport` = Somme(s1 + s2 + s3 + s4)
- `decaompte_esteme` = Kilos * Prix Estimé
- `ttcharges` = Dettes
- `ttdecompte` = `decaompte_esteme` - `ttcharges`

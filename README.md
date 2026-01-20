# MiniOrm

Mini ORM en **C#** permettant de traduire des requêtes LINQ en **SQL** et d’exécuter ces requêtes sur une base SQLite de manière sécurisée et typée.

Le projet illustre le fonctionnement interne d’un ORM minimaliste similaire à Entity Framework, en exposant les concepts de **LINQ Providers, Expression Trees, Reflection et Mapping objet-relationnel.**

---

## Fonctionnalités
- Mapping **fortement typé** entre classes C# et tables SQL via attributs [Table] et [Column].
- Traduction des expressions LINQ en **requêtes SQL** :
- Where → WHERE avec paramètres sécurisés (@p0, @p1…)
- Select → projections partielles (SELECT Id, Name)
- StartsWith → LIKE 'A%'
- Contains → LIKE '%text%'
- First / Take → limitation des résultats (LIMIT)
- Exécution des requêtes via **ADO.NET** (SqliteConnection, SqliteCommand, SqliteDataReader).
- **Materialisation** des résultats dans des objets C# ou des types anonymes.
- Support de **AND / OR / Comparaisons simples** (=, >, >=, <, <=).

---

## Structure du projet

```text
MiniOrm/
│
├── Core/                        ← Logique principale de l’ORM
│   ├── DbContext.cs             ← Point d’entrée, gestion de la connexion
│   ├── DbSet.cs                 ← Implémentation de IQueryable<T>
│   └── SqlQueryProvider.cs      ← Intercepte et exécute les expressions LINQ
│
├── Mapping/                     ← Mapping Classes ↔ Tables
│   ├── TableAttribute.cs        ← Attribut [Table] pour associer une classe à une table SQL
│   ├── ColumnAttribute.cs       ← Attribut [Column] pour associer une propriété à une colonne
│   ├── EntityMetadata.cs        ← Contient les métadonnées d’une entité : table, colonnes, types
│   └── EntityMapper.cs          ← Lit les métadonnées via reflection
│
├── Linq/                        ← Expression trees et traduction SQL
│   ├── SqlExpressionVisitor.cs  ← Analyse les expressions LINQ
│   ├── SqlQueryExecutor.cs      ← Exécute le SQL et matérialise les objets
│   ├── QueryTranslator.cs       ← Traduit un arbre d’expression LINQ complet en SqlWhereClause + projections + limites
│   └── SqlWhereClause.cs        ← Représente un WHERE clause SQL avec ses paramètres (@p0, @p1…) pour éviter les injections
│
├── Database/                    ← Initialisation et gestion SQLite
│   └── DatabaseInitializer.cs   ← Création des tables et données test
│   └── TableBuilder.cs          ← Fournit des méthodes pour créer dynamiquement des tables et colonnes via code
│
├── Entities/                    ← Entités mappées à la base
│   └── User.cs
│
└── Program.cs                   ← Exemple complet de tests et utilisation
```

---

## Exemple d’utilisation

```csharp
var db = new MiniDbContext();

// Users dont le nom commence par "A"
var usersStartsWithA = db.Set<User>()
                          .Where(u => u.Name.StartsWith("A"))
                          .ToList();

// Users avec "li" dans le nom, projection et Take
var usersContainsLi = db.Set<User>()
                         .Where(u => u.Name.Contains("li"))
                         .Select(u => new { u.Id, u.Name })
                         .Take(1)
                         .ToList();

// Utilisateurs majeurs
var adultUsers = db.Set<User>()
                   .Where(u => u.Age > 18)
                   .ToList();

// Projection Nom + Age
var nameAndAge = db.Set<User>()
                   .Where(u => u.Age >= 20)
                   .Select(u => new { u.Name, u.Age })
                   .ToList();
```

---

## Résultat attendu

```code 
Generated SQL:
SELECT id, name, age
FROM users
WHERE name LIKE @p0
Users dont le nom commence par 'A': Alice

Generated SQL:
SELECT id, name
FROM users
WHERE name LIKE @p0
LIMIT 1
Users avec 'li' dans le nom (limité à 1): Alice

Generated SQL:
SELECT id, name, age
FROM users
WHERE (age > @p0)
Users majeurs (age > 18):
1 - Alice (25)
3 - Charlie (30)

Generated SQL:
SELECT name, age
FROM users
WHERE (age >= @p0)
Projection Name + Age pour users >= 20:
Alice (25)
Charlie (30)
```

---

## Concepts clés appris
- **LINQ Providers** : interception des requêtes LINQ avant exécution.
- **Expression Trees** : analyse et traduction d’expressions u => u.Age > 18 && u.Name.StartsWith("A").
- **Reflection** : lecture des attributs [Table] et [Column] pour construire dynamiquement le mapping.
- **Materialisation** : transformation des résultats SQL en objets C# ou types anonymes.
- **Sécurité SQL**: utilisation de paramètres pour éviter les injections (@p0, @p1, etc.).
- **Projections et filtrage dynamique** : Select(), Where(), Take(), First().

---

## Tests réalisés
- StartsWith et Contains
- Select avec types anonymes
- Where avec opérateurs >, >=
- Take / First
- Récupération de tous les utilisateurs
- Projections partielles (new { u.Id, u.Name }, new { u.Name, u.Age })

---

## Prochaines améliorations possibles
- Support des OR et combinaisons plus complexes dans les Where.
- Gestion des relations (OneToMany, ManyToMany).
- Ajout du support pour OrderBy, Skip, Count.
- Migration vers d’autres bases SQL (MySQL, PostgreSQL).
- Implémentation de cache des objets pour éviter les requêtes redondantes.
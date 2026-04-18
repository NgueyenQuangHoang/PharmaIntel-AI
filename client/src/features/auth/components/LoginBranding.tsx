export function LoginBranding() {
  return (
    <div className="hidden md:flex flex-col justify-between p-12 ai-gradient-bg relative text-on-primary overflow-hidden w-full h-full">
      {/* Abstract AI Graphic / Pattern */}
      <div className="absolute inset-0 opacity-20 pointer-events-none mix-blend-overlay">
        <img alt="" className="w-full h-full object-cover" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuDMvxfGnFCY5V4feRhxpmLV4sJ1M955TSI1v-XcQdjDq7fu2d6h_aAc6tgXoZO3UROBR_OXs_pRKS0BpQcsX5XRRNPq8UxIG1On2AYVGtB97nl_vVqfdnDlhG3unNk7FEINGz1wMynUTsqIpVplpwsOQ4u1qnWR3ouTYH48fCI1xZSxVUGAOR07zM5B9Emzg2syF6xSr6t683udf4EGtftyz8m2TD1K7Uj7QGVzgEh8KOaLtYn6cNiFuyuyfWT38KBKObMr6lSP5fk" />
      </div>

      <div className="relative z-10">
        <div className="flex items-center gap-2 mb-16">
          <span className="material-symbols-outlined text-3xl fill-icon">biotech</span>
          <span className="font-headline font-bold text-2xl tracking-tighter">PharmaIntel AI</span>
        </div>
        <h1 className="font-headline font-extrabold text-5xl leading-tight mb-6 tracking-tight">
          Clinical Intelligence,<br/>Accelerated.
        </h1>
        <p className="text-on-primary/80 font-body text-lg leading-relaxed max-w-md">
          Access predictive modeling, regulatory compliance insights, and high-science precision tools tailored for pharmaceutical research.
        </p>
      </div>

      <div className="relative z-10 flex items-center gap-4 mt-16 p-6 rounded-lg bg-surface-container-lowest/10 backdrop-blur-md ghost-border">
        <div className="flex -space-x-4">
          <img alt="Researcher" className="w-12 h-12 rounded-full border-2 border-primary-container object-cover" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuB02AbYI7SSww70lTQQqzzdAHrr6PW61lHlch1WIg3aHVaJXSyTuEb6glrUS-aDHxyWwsZYJyegxEPtKrarrOtX3f5_nJbey2bhSjoEQU7RpH1tvB88aklQ5v2DMO_nTEFHDc88nOPR0vSI4dm-7W6i58DDpeLjDJT9QiaYHNLchPyAq_dHAhpqjqfVO-j8wjy4YwROeZ05dwWK4FQNnBLcOE3XOHQXzjYkfB8VGrgkPRYuInUFtajTwgj8xortlvP5EJLD_IzQbP0" />
          <img alt="Doctor" className="w-12 h-12 rounded-full border-2 border-primary-container object-cover" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBIGMAe2d9fHO8nMWlOfKgS9U_0W8Z9_KoNN39MXZQmuafD5IIs-Fnqo8GgkicZP-XrthD99heeHUzXJiCW2bu8LYoezlTz59aV92lnNZ0gah_PQBQlARo2oQf96SxsJZrVWe8dUrdrnQp3qveL0K5TSnBjrOtZVbSznx3aJkQ-bTRaUBvqUk7w9TAGFRQA0CVT8vC9KNbbPQ5uxEjggjwrPa1gMt6x5_3aS5Bl8O-Yhbrwm4YgHLy4O8fQtilJ1FAkFmzCIqxumVI" />
          <img alt="Data Scientist" className="w-12 h-12 rounded-full border-2 border-primary-container object-cover" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBXnI7KC1o6RxCqNnUr4O8slL8XxaNbFA38DMOR8FNeCvilDwV0EElwJMB5gis213SwEMf2pZl7MphiaXWZASIR1SKqOEypKKmLr9AdVOZ4vtwFfwmWwyQDWnDW6WhpZQZ6hNkAgGc0DItUijVcbVfkLcbntDb67wM69INSJ-Nmb-tFMtuZJEeazjkPhQbpsUJBCGx1w64Fjr2-InXYDE6bpomKuek_1F7R044kNhdXl21bclL0IsCdatQm0Q4A1bscFXq0cQIho3Y" />
        </div>
        <div className="ml-4">
          <p className="font-headline font-bold text-sm">Trusted by 10,000+ researchers</p>
          <p className="text-sm text-on-primary/70">Top pharmaceutical institutions</p>
        </div>
      </div>
    </div>
  );
}

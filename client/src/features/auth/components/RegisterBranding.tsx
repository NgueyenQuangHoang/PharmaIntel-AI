export function RegisterBranding() {
  return (
    <div className="hidden lg:flex relative overflow-hidden bg-surface-container-low flex-col justify-between p-12 w-full h-full">
      {/* Background Image with Ambient Overlay */}
      <div className="absolute inset-0 z-0">
        <img alt="Clinical AI Visualization" className="w-full h-full object-cover opacity-80 mix-blend-luminosity" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuD7CQyv2o7Ffh3Tt4T59KPiyP2SJPy4D3yy8vVMWgKsleQ7wik9mkSJvHFiBzBWnKkznd3nEpck6RZNQf1Q56i4TLM763zrLt-SNbAlGv5F6zbfIdB-z7RVviN6ntZw7Ov8_Z3TnmNuwkhv7mddXpfZy048ki9wO9E2c8fdg_SdOJmSC7JrJuo3uEYw0servkOiUII9Or0buXJB42f1yqlairnaLSYB-vuaMTiO4uXnY2bmx_Z9V-dMOMM8xxl2Cc4m0GRx5ks8mdo" />
        {/* Brand Gradient Texture */}
        <div className="absolute inset-0 bg-gradient-to-br from-primary/90 to-primary-container/80 mix-blend-multiply"></div>
      </div>

      {/* Content Over Visual */}
      <div className="relative z-10">
        <div className="text-xl font-headline font-bold tracking-tighter text-on-primary">PharmaIntel AI</div>
      </div>
      
      <div className="relative z-10 max-w-md">
        <h2 className="font-headline text-4xl font-bold text-on-primary mb-4 leading-tight tracking-tight">
          Next-Generation Clinical Intelligence
        </h2>
        <p className="font-body text-primary-fixed text-lg">
          Join our platform to accelerate research with AI-driven compliance and data analysis tools designed for modern life sciences.
        </p>
      </div>
      
      <div className="relative z-10 flex items-center space-x-2 text-primary-fixed text-sm font-label">
        <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>security</span>
        <span>Enterprise Grade Security &amp; Compliance</span>
      </div>
    </div>
  );
}
